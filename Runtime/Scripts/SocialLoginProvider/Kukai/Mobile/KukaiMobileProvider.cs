using System;
using System.Collections.Generic;
using Netezos.Encoding;
using Tezos.Configs;
using Tezos.Cysharp;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.Operation;
using Tezos.Request;
using UnityEngine;
using SignPayloadType = Tezos.Operation.SignPayloadType;

namespace Tezos.SocialLoginProvider
{
	public class KukaiMobileProvider : IAndroidProvider, IiOSProvider
	{
		public AuthResponse       AuthResponse       { get; private set; }
		public SocialProviderData SocialProviderData { get; set; }
		public SocialLoginType    SocialLoginType    => SocialLoginType.Kukai;

		private readonly UrlParser _urlParser = new();

		private string _network => ConfigGetter.GetOrCreateConfig<TezosConfig>().Network.ToString();

		private UniTaskCompletionSource<OperationResponse>   _operationTcs;
		private UniTaskCompletionSource<SignPayloadResponse> _signPayloadTcs;

		private UrlGenerator                                _urlGenerator;
		private UniTaskCompletionSource<SocialProviderData> _logInTcs;
		private UniTaskCompletionSource<bool>               _logOutTcs;
		private TypeOfLogin                                 _typeOfLogin;
		private string                                      _webClientAddress;
		private Rpc                                         _rpc;
		private TezosConfig                                 _tezosConfig;

		public UniTask Init(SocialProviderController socialLoginController)
		{
			_tezosConfig       = ConfigGetter.GetOrCreateConfig<TezosConfig>();
			_rpc               = new(_tezosConfig.RequestTimeoutSeconds);
			_urlGenerator      = new UrlGenerator(ConfigGetter.GetOrCreateConfig<TezosConfig>().KukaiWebClientAddress);
			SocialProviderData = socialLoginController.GetSocialProviderData();
			InitializeDeepLinking();

			return UniTask.CompletedTask;
		}

		public UniTask<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			TezosLogger.LogDebug("Login entered");
			if (_logInTcs != null && _logInTcs.Task.Status == UniTaskStatus.Pending) return _logInTcs.Task;

			TezosLogger.LogDebug("Initiating wallet connection.");
			_logInTcs = new();
			OpenLoginLink();
			return _logInTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000, "Kukai login timed out.");
		}

		public UniTask<bool> LogOut()
		{
			if (_logOutTcs != null && _logOutTcs.Task.Status == UniTaskStatus.Pending) return _logOutTcs.Task;

			TezosLogger.LogDebug("Wallet disconnected.");
			_logOutTcs         = new();
			SocialProviderData = null;
			_logOutTcs.TrySetResult(true);
			return _logOutTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000);
		}

		public UniTask<string> GetBalance(string walletAddress) => _rpc.GetRequest<string>(EndPoints.GetBalanceEndPoint(walletAddress));

		public bool IsLoggedIn() => !string.IsNullOrEmpty(SocialProviderData?.WalletAddress);

		private static TypeOfLogin ParseTypeOfLogin(string loginType)
		{
			if (Enum.TryParse<TypeOfLogin>(loginType, true, out var result))
			{
				return result;
			}

			throw new ArgumentException($"Invalid login type: {loginType}");
		}

		private void HandleLoginDeepLink(ParsedURLData parsedData)
		{
			if (_logInTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Deeplink received but probably task is timed out.");
				return;
			}

			TezosLogger.LogDebug("Handling login response.");
			_typeOfLogin = ParseTypeOfLogin(parsedData.GetParameter("typeOfLogin"));

			SocialProviderData = new SocialProviderData
			                     {
				                     SocialLoginType = SocialLoginType.Kukai,
				                     WalletAddress   = parsedData.GetParameter("address"),
				                     PublicKey       = parsedData.GetParameter("public_key"),
				                     LoginType       = _typeOfLogin,
				                     LoginDetails    = parsedData.GetParameter("login_details"),
			                     };

			AuthResponse = new AuthResponse
			               {
				               Message   = parsedData.GetParameter("message"),
				               Signature = parsedData.GetParameter("signature")
			               };

			_logInTcs.TrySetResult(SocialProviderData);
		}

		private void HandleOperationDeepLink(ParsedURLData parsedData)
		{
			if (_operationTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Deeplink received but probably task is timed out.");
				return;
			}

			TezosLogger.LogDebug("Handling operation response.");

			var operationResult = new OperationResponse
			                      {
				                      TransactionHash = parsedData.GetParameter("operationHash"),
			                      };
			_operationTcs.TrySetResult(operationResult);
		}

		private void HandleSignExpressionDeepLink(ParsedURLData parsedData)
		{
			if (_signPayloadTcs.Task.Status == UniTaskStatus.Faulted)
			{
				TezosLogger.LogWarning("Deeplink received but probably task is timed out.");
				return;
			}

			TezosLogger.LogDebug("Handling sign expression request.");

			var signResult = new SignPayloadResponse
			                 {
				                 Id        = parsedData.GetParameter("expression"),
				                 Signature = parsedData.GetParameter("operationHash")
			                 };

			TezosLogger.LogDebug($"Dispatching payload signed event for expression: {parsedData.GetParameter("expression")} and signature: {signResult.Signature}");
			_signPayloadTcs.TrySetResult(signResult);
		}

		private ParsedURLData ParseDeepLinkUrl(string url)
		{
			TezosLogger.LogDebug($"Parsing deep link URL: {url}");

			var parsedData = _urlParser.ParseURL(url);

			if (parsedData == null)
			{
				TezosLogger.LogError($"Error parsing URL: {url}");
			}

			return parsedData;
		}

		private void OnDeepLinkActivated(string url)
		{
			TezosLogger.LogDebug($"Deep link activated: {url}");

			var parsedData = ParseDeepLinkUrl(url);

			if (parsedData == null)
			{
				TezosLogger.LogError($"Error parsing deep link URL: {url}");
				return;
			}

			var requestType = parsedData.GetParameter("type");
			TezosLogger.LogDebug($"Request type: {requestType}");

			// Check for error parameters
			var errorMessage = parsedData.GetParameter("errorMessage");
			var errorId      = parsedData.GetParameter("errorId");

			if (!string.IsNullOrEmpty(errorMessage))
			{
				HandleErrorDeepLink(errorMessage, requestType, errorId);
				return;
			}

			TezosLogger.LogDebug($"Deep link request type: {requestType}");
			switch (requestType)
			{
				case ActionTypes.LOGIN:
					HandleLoginDeepLink(parsedData);
					break;
				case ActionTypes.OPERATION:
					HandleOperationDeepLink(parsedData);
					break;
				case ActionTypes.SIGN:
					HandleSignExpressionDeepLink(parsedData);
					break;
				case "originate":
					// HandleOriginationDeepLink(parsedData);
					break;
				default:
					TezosLogger.LogWarning($"Unknown request type: {parsedData.GetParameter("type")}");
					break;
			}
		}

		private void HandleErrorDeepLink(string errorMessage, string action, string errorId) { TezosLogger.LogError($"Error received from Kukai: {errorMessage}, Action: {action}, Error ID: {errorId}"); }

		private void InitializeDeepLinking()
		{
			Application.deepLinkActivated += OnDeepLinkActivated;
			var initialURL = Application.absoluteURL; // For Android, iOS, or Universal Windows Platform (UWP) this is a deep link URL (Read Only).

			if (!string.IsNullOrEmpty(initialURL))
			{
				// Cold start and Application.absoluteURL not null so process Deep Link.
				OnDeepLinkActivated(initialURL);
			}
		}

		private void OpenLoginLink()
		{
			var loginLink = _urlGenerator.GenerateLoginLink("my_nonce", "my_project_id", _network);

			OpenLink(loginLink);
		}

		private static void OpenLink(string linkToOpen)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			var safeBrowsing = UniWebViewSafeBrowsing.Create(loginLink);
			safeBrowsing.Show();
#else
			Application.OpenURL(linkToOpen);
#endif
		}

		private void TestOperation()
		{
			TezosLogger.LogDebug("TestOperation");

			var parameter = new MichelinePrim
			                {
				                Prim = PrimType.Pair,
				                Args = new List<IMicheline>
				                       {
					                       new MichelineInt(11000856),
					                       new MichelinePrim
					                       {
						                       Prim = PrimType.Pair,
						                       Args = new List<IMicheline>
						                              {
							                              new MichelineInt(1),
							                              new MichelinePrim
							                              {
								                              Prim = PrimType.Pair,
								                              Args = new List<IMicheline>
								                                     {
									                                     new MichelinePrim { Prim = PrimType.None },
									                                     new MichelinePrim
									                                     {
										                                     Prim = PrimType.Pair,
										                                     Args = new List<IMicheline>
										                                            {
											                                            new MichelinePrim { Prim = PrimType.None },
											                                            new MichelineArray()
										                                            }
									                                     }
								                                     }
							                              }
						                              }
					                       }
				                       }
			                }.ToJson();

			const string _entry_point = "fulfill_ask";
			const string _destination = "KT1MFWsAXGUZ4gFkQnjByWjrrVtuQi4Tya8G";
			const string _amount      = "1500000";

			var req = new OperationRequest
			          {
				          Destination = _destination,
				          EntryPoint  = _entry_point,
				          Arg         = parameter,
				          Amount      = _amount
			          };

			OpenOperationLink(req);
		}

		private void OpenOperationLink(OperationRequest request)
		{
			if (SocialProviderData == null || string.IsNullOrEmpty(SocialProviderData.WalletAddress))
			{
				TezosLogger.LogError("No active wallet found");
				return;
			}

			var operationLink = _urlGenerator.GenerateOperationLink(request, SocialProviderData.WalletAddress, SocialProviderData.LoginType, _network);
			Debug.Log($"operationLink:{operationLink}");

			OpenLink(operationLink);
		}

		public async UniTask<OperationResponse> RequestOperation(OperationRequest operationRequest)
		{
			if (_operationTcs != null && _operationTcs.Task.Status == UniTaskStatus.Pending) return await _operationTcs.Task;
			_operationTcs = new();

			TezosLogger.LogDebug("Requesting operation.");
			OpenOperationLink(operationRequest);
			return await _operationTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000);
		}

		public async UniTask<SignPayloadResponse> RequestSignPayload(SignPayloadRequest signPayloadRequest)
		{
			if (_signPayloadTcs != null && _signPayloadTcs.Task.Status == UniTaskStatus.Pending) return await _signPayloadTcs.Task;
			_signPayloadTcs = new();

			signPayloadRequest.SigningType = Enum.Parse<SignPayloadType>(signPayloadRequest.SigningType.ToString().ToLowerInvariant());
			var signLink = _urlGenerator.GenerateSignLink(signPayloadRequest, _typeOfLogin, _network);

			OpenLink(signLink);

			return await _signPayloadTcs.WithTimeout(_tezosConfig.RequestTimeoutSeconds * 1000);
		}

		public UniTask RequestContractOrigination(DeployContractRequest deployContractRequest) => throw new NotSupportedException("Contract origination is not supported by Kukai wallet.");
	}
}