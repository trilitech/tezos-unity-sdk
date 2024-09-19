using System;
using System.Collections.Generic;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Encoding;
using Tezos.Configs;
using Tezos.Cysharp.Threading.Tasks;
using Tezos.Logger;
using Tezos.MessageSystem;
using Tezos.WalletProvider;
using Tezos.WalletProvider.Types;
using UnityEngine;

namespace Tezos.SocialLoginProvider
{
	public class KukaiProvider : ISocialLoginProvider
	{
		public AuthResponse AuthResponse { get; private set; }
		public SocialLoginType SocialLoginType => SocialLoginType.Kukai;
		
		private readonly UrlParser    _urlParser = new();
		
		private UrlGenerator                                _urlGenerator;
		private SocialProviderData                          _socialProviderData;
		private UniTaskCompletionSource<SocialProviderData> _logInTcs;
		private UniTaskCompletionSource<bool>               _logOutTcs;
		private TypeOfLogin                                 _typeOfLogin;
		private string                                      _webClientAddress;

		public UniTask Init(SocialLoginController socialLoginController)
		{
			_urlGenerator = new UrlGenerator(ConfigGetter.GetOrCreateConfig<TezosConfig>().KukaiWebClientAddress);
			InitializeDeepLinking();
			
			return UniTask.CompletedTask;
		}

		public UniTask<SocialProviderData> LogIn(SocialProviderData socialProviderData)
		{
			TezosLogger.LogDebug("Login entered");
			if(_logInTcs != null && _logInTcs.Task.Status == UniTaskStatus.Pending)
				return _logInTcs.Task;
			
			TezosLogger.LogDebug("Initiating wallet connection.");
			_logInTcs = new();
			OpenLoginLink();
			OnDeepLinkActivated("unitydl001://kukai-embed/?type=login&address=tz2Br3myzfDe1L3W4xZoaxVv3CkXzn5ryZyA&public_key=sppk7bLPzXaC1EteF9m1gcCavpJXyHrG8HtcE7tf77ZXZ37srHT5RRU&name=Talha%20%C3%87a%C4%9Fatay%20I%C5%9Fik&email=talha.isik@trili.tech&message=Tezos%20Signed%20Message:%20%7B%22requestId%22:%22sample-id%22,%22purpose%22:%22authentication%22,%22currentTime%22:%221726225807%22,%22nonce%22:%22my_nonce%22,%22network%22:%22ghostnet%22,%22publicKey%22:%22sppk7bLPzXaC1EteF9m1gcCavpJXyHrG8HtcE7tf77ZXZ37srHT5RRU%22,%22address%22:%22tz2Br3myzfDe1L3W4xZoaxVv3CkXzn5ryZyA%22,%22domain%22:%22http://localhost:3000%22%7D&signature=spsig1FE2k1BARDdXDKxYCaNMcduwprkasgQSp2Xm3Be83eedAH6RrskFASyXPxbnByJdHk4eYuHFuxTP9c9sNg2ysPC8M8oAtG&typeOfLogin=google");
			return _logInTcs.Task;
		}

		public string GetWalletAddress() => _socialProviderData?.WalletAddress;

		public UniTask<bool> LogOut()
		{
			if(_logOutTcs != null && _logOutTcs.Task.Status == UniTaskStatus.Pending)
				return _logOutTcs.Task;
			
			TezosLogger.LogDebug("Wallet disconnected.");
			_logOutTcs = new();
			_socialProviderData = null;
			_logOutTcs.TrySetResult(true);
			return _logOutTcs.Task;
		}

		public bool IsLoggedIn() => !string.IsNullOrEmpty(_socialProviderData?.WalletAddress);

		public void RequestOperation(SocialOperationRequest operationRequest)
		{
			TezosLogger.LogDebug("Requesting operation.");
			OpenOperationLink(operationRequest);
		}

		public void RequestSignPayload(SocialSignPayloadRequest signRequest)
		{
			signRequest.SigningType = SignPayloadType.raw;
			var signLink = _urlGenerator.GenerateSignLink(signRequest, _typeOfLogin);
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
			_typeOfLogin = ParseTypeOfLogin(parsedData.GetParameter("typeOfLogin"));

			_socialProviderData = new SocialProviderData
			{
				SocialLoginType = SocialLoginType.Kukai,
				WalletAddress   = parsedData.GetParameter("address"),
				PublicKey       = parsedData.GetParameter("public_key"),
				LoginType       = _typeOfLogin.ToString()
			};

			AuthResponse = new AuthResponse
			{
				Message = parsedData.GetParameter("message"),
				Signature = parsedData.GetParameter("signature")
			};

			_logInTcs.TrySetResult(_socialProviderData);
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

			var operationLink = _urlGenerator.GenerateOperationLink(request, _socialProviderData.WalletAddress, _typeOfLogin);
			Debug.Log($"operationLink:{operationLink}");
			Application.OpenURL(operationLink);
		}
	}

}