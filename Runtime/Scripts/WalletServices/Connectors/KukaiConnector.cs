using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon;
using Netezos.Encoding;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;
using UnityEngine;

namespace TezosSDK.WalletServices.Connectors
{

	public class KukaiConnector : IWalletConnector
	{
		private const string DEEP_LINK_URL = "http://192.168.0.74:3000";

		private readonly EventDispatcher _eventDispatcher;
		private readonly UrlParser _urlParser = new();
		private readonly UrlGenerator _urlGenerator = new(DEEP_LINK_URL);
		private WalletInfo _activeWallet; // Keep track of the active wallet
		private string _typeOfLogin;

		public KukaiConnector(WalletEventManager eventManager)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
			ConnectorType = ConnectorType.Kukai;

			InitializeDeepLinking();

			OnDeepLinkActivated(
				"unitydl001://kukai-embed/?type=operation_response&address=tz2NRuiGPR9FGJ6oBDzE6Uqxf3CVosHcHeem&name=can%20berk%20turakan&email=can.berk.turakan2@gmail.com&typeOfLogin=google&operation_hash=oo3hKEBwgawUNEwKPjEFeESaL9av52uo3dDRsssvXAxndd73jks");
		}

		public void Dispose()
		{
		}

		public ConnectorType ConnectorType { get; }

		public event Action<WalletMessageType> OperationRequested;

		public void ConnectWallet()
		{
			TezosLogger.LogDebug("Initiating wallet connection.");
			OpenLoginLink();
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
			throw new NotImplementedException("Not yet implemented");
		}

		public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			throw new NotSupportedException("Contract origination is not supported by Kukai wallet.");
		}

		public Task InitializeAsync()
		{
			return Task.CompletedTask;
		}

		private void HandleLogin(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling login response.");

			var wallet = new WalletInfo
			{
				Address = parsedData.GetParameter("address"),
				PublicKey = parsedData.GetParameter("public_key")
			};

			_activeWallet = wallet;
			_eventDispatcher.DispatchWalletConnectedEvent(wallet);
			_typeOfLogin = parsedData.GetParameter("typeOfLogin");
		}

		private void HandleOperation(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling operation response.");

			var operation = new OperationInfo(parsedData.GetParameter("operation_hash"), parsedData.GetParameter("operation_hash"), BeaconMessageType.operation_response);

			TezosLogger.LogDebug($"Dispatching operation injected event with operation hash: {operation.Hash}");
			_eventDispatcher.DispatchOperationInjectedEvent(operation);
		}

		private void HandleSign(ParsedURLData parsedData)
		{
			TezosLogger.LogDebug("Handling sign request.");
			throw new NotImplementedException("Signing is not supported by Kukai wallet.");
		}

		private void HandleOrigination(ParsedURLData parsedData)
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

			switch (requestType)
			{
				case "login":
					HandleLogin(parsedData);
					break;
				case "operation_response":
					HandleOperation(parsedData);
					break;
				case "sign":
					HandleSign(parsedData);
					break;
				case "originate":
					HandleOrigination(parsedData);
					break;
				default:
					TezosLogger.LogWarning($"Unknown request type: {parsedData.GetParameter("type")}");
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

				_activeWallet = new WalletInfo
				{
					Address = "tz2NRuiGPR9FGJ6oBDzE6Uqxf3CVosHcHeem"
				};
			}

			if (string.IsNullOrEmpty(_typeOfLogin))
			{
				TezosLogger.LogError("No type of login found");
				_typeOfLogin = "google";
			}

			var operationLink = _urlGenerator.GenerateOperationLink(request, _activeWallet.Address, _typeOfLogin);
			Application.OpenURL(operationLink);
		}
	}

}