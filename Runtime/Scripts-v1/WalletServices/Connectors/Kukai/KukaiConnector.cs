using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using Netezos.Encoding;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Connectors.Kukai.Helpers;
using TezosSDK.WalletServices.Connectors.Kukai.Types;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;
using TezosSDK.WalletServices.Interfaces;
using UnityEngine;

namespace TezosSDK.WalletServices.Connectors.Kukai
{

	public class KukaiConnector : IWalletConnector
	{
		private readonly UrlGenerator _urlGenerator;
		private readonly UrlParser    _urlParser = new();
		
		private EventDispatcher _eventDispatcher;
		private WalletInfo      _activeWallet;
		private string          _webClientAddress;

		public KukaiConnector()
		{
			_urlGenerator = new UrlGenerator(TezosManager.Instance.Config.KukaiWebClientAddress);
			ConnectorType = ConnectorType.Kukai;

			InitializeDeepLinking();

			// OnDeepLinkActivated("unitydl001://kukai-embed/?type=operation_response&address=tz2NRuiGPR9FGJ6oBDzE6Uqxf3CVosHcHeem&name=can%20berk%20turakan&email=can.berk.turakan2@gmail.com&typeOfLogin=google&operation_hash=oo3hKEBwgawUNEwKPjEFeESaL9av52uo3dDRsssvXAxndd73jks");
		}

		public TypeOfLogin TypeOfLogin { get; private set; }

		public AuthResponse AuthResponse { get; private set; }

		public void Dispose()
		{
		}

		public ConnectorType ConnectorType { get; }

		public PairingRequestData PairingRequestData
		{
			get => null;
		}

		public event Action<WalletMessageType> OperationRequested;

		public void ConnectWallet()
		{
			TezosLogger.LogDebug("Initiating wallet connection.");
			OpenLoginLink();
			// TestOperation();
		}

		public string GetWalletAddress()
		{
			return _activeWallet?.Address;
		}

		public void DisconnectWallet()
		{
			TezosLogger.LogDebug("Disconnecting wallet.");
			var wallet = _activeWallet;
			_activeWallet = null;
			_eventDispatcher.DispatchWalletDisconnectedEvent(wallet);
			TezosLogger.LogDebug("Wallet disconnected.");
		}

		public void RequestOperation(WalletOperationRequest operationRequest)
		{
			TezosLogger.LogDebug("Requesting operation.");
			OpenOperationLink(operationRequest);
		}

		public void RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			signRequest.SigningType = SignPayloadType.raw;
			var signLink = _urlGenerator.GenerateSignLink(signRequest, TypeOfLogin);
			Application.OpenURL(signLink);
		}

		public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			throw new NotSupportedException("Contract origination is not supported by Kukai wallet.");
		}

		public Task InitializeAsync(IWalletEventManager eventManager)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
			_activeWallet    = TezosManager.Instance.WalletConnection.WalletInfo;
			return Task.CompletedTask;
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

			var wallet = new WalletInfo
			{
				ConnectorType = ConnectorType.Kukai,
				Address       = parsedData.GetParameter("address"),
				PublicKey     = parsedData.GetParameter("public_key")
			};

			_activeWallet = wallet;
			TypeOfLogin = ParseTypeOfLogin(parsedData.GetParameter("typeOfLogin"));

			AuthResponse = new AuthResponse
			{
				Message = parsedData.GetParameter("message"),
				Signature = parsedData.GetParameter("signature")
			};

			_eventDispatcher.DispatchWalletConnectedEvent(wallet);
		}

		private void HandleOperationDeepLink(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling operation response.");

			var operation = new OperationInfo(parsedData.GetParameter("operationHash"), parsedData.GetParameter("operationHash"), BeaconMessageType.operation_response);

			TezosLogger.LogDebug($"Dispatching operation injected event with operation hash: {operation.Hash}");
			_eventDispatcher.DispatchOperationInjectedEvent(operation);
		}

		private void HandleSignExpressionDeepLink(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling sign expression request.");

			var signResult = new SignResult
			{
				Message = parsedData.GetParameter("expression"),
				Signature = parsedData.GetParameter("signature")
			};

			TezosLogger.LogDebug($"Dispatching payload signed event for expression: {signResult.Message} and signature: {signResult.Signature}");
			_eventDispatcher.DispatchPayloadSignedEvent(signResult);

			var verification = TezosManager.Instance.Tezos.WalletTransaction.VerifySignedPayload(SignPayloadType.raw, signResult.Signature);
		}

		private void HandleOriginationDeepLink(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling contract origination request.");
			throw new NotImplementedException("Contract origination is not supported by Kukai wallet.");
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
					HandleOriginationDeepLink(parsedData);
					break;
				default:
					TezosLogger.LogWarning($"Unknown request type: {parsedData.GetParameter("type")}");
					break;
			}
		}

		private void HandleErrorDeepLink(string errorMessage, string action, string errorId)
		{
			TezosLogger.LogError($"Error received from Kukai: {errorMessage}, Action: {action}, Error ID: {errorId}");

			switch (action)
			{
				case ActionTypes.LOGIN:
					_eventDispatcher.DispatchOperationFailedEvent(new OperationInfo("", "", BeaconMessageType.permission_request, errorMessage));
					break;
				case ActionTypes.OPERATION:
					_eventDispatcher.DispatchOperationFailedEvent(new OperationInfo("", "", BeaconMessageType.operation_request, errorMessage));
					break;
				case ActionTypes.SIGN:
					_eventDispatcher.DispatchOperationFailedEvent(new OperationInfo("", "", BeaconMessageType.sign_payload_request, errorMessage));
					break;
				case "originate":
					_eventDispatcher.DispatchOperationFailedEvent(new OperationInfo("", "", BeaconMessageType.operation_request, errorMessage));
					break;
				default:
					TezosLogger.LogWarning($"Unknown action type: {action}");
					_eventDispatcher.DispatchOperationFailedEvent(new OperationInfo("", "", BeaconMessageType.error, errorMessage));
					break;
			}
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

			var req = new WalletOperationRequest
			{
				Destination = _destination,
				EntryPoint = _entry_point,
				Arg = parameter,
				Amount = _amount
			};

			OpenOperationLink(req);
		}

		private void OpenOperationLink(WalletOperationRequest request)
		{
			if (_activeWallet == null || string.IsNullOrEmpty(_activeWallet.Address))
			{
				TezosLogger.LogError("No active wallet found");
				return;
			}

			var operationLink = _urlGenerator.GenerateOperationLink(request, _activeWallet.Address, TypeOfLogin);
			Debug.Log($"operationLink:{operationLink}");
			Application.OpenURL(operationLink);
		}
	}

}