using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Encoding;
using TezosSDK.Configs;
using TezosSDK.Logger;
using TezosSDK.MessageSystem;
using TezosSDK.WalletProvider;
using TezosSDK.WalletProvider.Types;
using UnityEngine;

namespace TezosSDK.SocialLoginProvider
{
	public class KukaiProvider : ISocialLoginProvider
	{
		private readonly UrlParser    _urlParser = new();
		
		private UrlGenerator                             _urlGenerator;
		private SocialProviderData                       _socialProviderData;
		private TaskCompletionSource<SocialProviderData> _logInTcs;
		private TaskCompletionSource<bool>               _logOutTcs;
		private string                                   _webClientAddress;

		public Task Init(SocialLoginController socialLoginController)
		{
			_urlGenerator = new UrlGenerator(ConfigGetter.GetOrCreateConfig<TezosConfig>().KukaiWebClientAddress);
			InitializeDeepLinking();
			
			return Task.CompletedTask;
		}

		public TypeOfLogin TypeOfLogin { get; private set; }

		public AuthResponse AuthResponse { get; private set; }

		public SocialLoginType SocialLoginType => SocialLoginType.Kukai;

		public Task<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			if(_logInTcs != null && _logInTcs.Task.Status == TaskStatus.Running)
				return _logInTcs.Task;
			
			TezosLogger.LogDebug("Initiating wallet connection.");
			_logInTcs = new();
			OpenLoginLink();
			return _logInTcs.Task;
		}

		public string GetWalletAddress() => _socialProviderData?.WalletAddress;

		public Task<bool> LogOut()
		{
			if(_logOutTcs != null && _logOutTcs.Task.Status == TaskStatus.Running)
				return _logOutTcs.Task;
			
			TezosLogger.LogDebug("Wallet disconnected.");
			_logOutTcs = new();
			_socialProviderData = null;
			_logOutTcs.SetResult(true);
			return _logOutTcs.Task;
		}

		public void RequestOperation(SocialOperationRequest operationRequest)
		{
			TezosLogger.LogDebug("Requesting operation.");
			OpenOperationLink(operationRequest);
		}

		public void RequestSignPayload(SocialSignPayloadRequest signRequest)
		{
			signRequest.SigningType = SignPayloadType.raw;
			var signLink = _urlGenerator.GenerateSignLink(signRequest, TypeOfLogin);
			Application.OpenURL(signLink);
		}

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
			TezosLogger.LogDebug("Handling login response.");

			_socialProviderData = new SocialProviderData
			{
				SocialLoginType = SocialLoginType.Kukai,
				WalletAddress   = parsedData.GetParameter("address"),
				PublicKey       = parsedData.GetParameter("public_key")
			};

			TypeOfLogin = ParseTypeOfLogin(parsedData.GetParameter("typeOfLogin"));

			AuthResponse = new AuthResponse
			{
				Message = parsedData.GetParameter("message"),
				Signature = parsedData.GetParameter("signature")
			};

			_logInTcs.SetResult(_socialProviderData);
		}

		private void HandleOperationDeepLink(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling operation response.");

			// var operation = new OperationInfo(parsedData.GetParameter("operationHash"), parsedData.GetParameter("operationHash"), BeaconMessageType.operation_response);

			// TezosLogger.LogDebug($"Dispatching operation injected event with operation hash: {operation.Hash}");
			// _eventDispatcher.DispatchOperationInjectedEvent(operation);
		}

		private void HandleSignExpressionDeepLink(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling sign expression request.");

			var signResult = new SocialSignData
			{
				Message = parsedData.GetParameter("expression"),
				Signature = parsedData.GetParameter("signature")
			};

			TezosLogger.LogDebug($"Dispatching payload signed event for expression: {signResult.Message} and signature: {signResult.Signature}");
			// _eventDispatcher.DispatchPayloadSignedEvent(signResult);
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
			var errorId = parsedData.GetParameter("errorId");

			if (!string.IsNullOrEmpty(errorMessage))
			{
				HandleErrorDeepLink(errorMessage, requestType, errorId);
				return;
			}

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

		private void HandleErrorDeepLink(string errorMessage, string action, string errorId)
		{
			TezosLogger.LogError($"Error received from Kukai: {errorMessage}, Action: {action}, Error ID: {errorId}");
			_socialProviderData.Error = action + "-" + errorMessage + "-" + errorId;
		}

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
			var loginLink = _urlGenerator.GenerateLoginLink("my_nonce", "my_project_id");
			Application.OpenURL(loginLink);
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
									new MichelinePrim
									{
										Prim = PrimType.None
									},
									new MichelinePrim
									{
										Prim = PrimType.Pair,
										Args = new List<IMicheline>
										{
											new MichelinePrim
											{
												Prim = PrimType.None
											},
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
			const ulong _amount = 1500000;

			var req = new SocialOperationRequest
			{
				Destination = _destination,
				EntryPoint = _entry_point,
				Arg = parameter,
				Amount = _amount
			};

			OpenOperationLink(req);
		}

		private void OpenOperationLink(SocialOperationRequest request)
		{
			if (_socialProviderData == null || string.IsNullOrEmpty(_socialProviderData.WalletAddress))
			{
				TezosLogger.LogError("No active wallet found");
				return;
			}

			var operationLink = _urlGenerator.GenerateOperationLink(request, _socialProviderData.WalletAddress, TypeOfLogin);
			Debug.Log($"operationLink:{operationLink}");
			Application.OpenURL(operationLink);
		}
	}

}