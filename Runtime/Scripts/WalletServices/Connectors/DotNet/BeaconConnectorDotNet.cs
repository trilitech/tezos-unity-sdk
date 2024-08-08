using System;
using System.Threading.Tasks;
using Beacon.Sdk.Beacon;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Models;
using TezosSDK.WalletServices.Beacon;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;
// ReSharper disable once RedundantUsingDirective
using TezosSDK.Helpers;
using TezosSDK.WalletServices.Interfaces;
// ReSharper disable once RedundantUsingDirective
using UnityEngine;

namespace TezosSDK.WalletServices.Connectors.DotNet
{

	public class BeaconConnectorDotNet : IWalletConnector
	{
		private readonly OperationRequestHandler _operationRequestHandler;
	
		private BeaconClientManager _beaconClientManager;
		private IWalletEventManager  _eventManager;

		public BeaconConnectorDotNet()
		{
			_operationRequestHandler             =  new OperationRequestHandler();
			_operationRequestHandler.MessageSent += OnBeaconMessageSent;
			ConnectorType                        =  ConnectorType.BeaconDotNet;
		}

		public async Task InitializeAsync(IWalletEventManager eventManager)
		{
			_eventManager                    =  eventManager;
			_eventManager.PairingRequested   += OnPairingRequested;
			_eventManager.WalletDisconnected += OnWalletDisconnected;
			_beaconClientManager             =  new BeaconClientManager(eventManager, _operationRequestHandler);
			await _beaconClientManager.CreateAsync();
		}

		public void Dispose()
		{
			_beaconClientManager.Dispose();
		}

		public ConnectorType ConnectorType { get; }
		public PairingRequestData PairingRequestData { get; private set; }

		public event Action<WalletMessageType> OperationRequested;

		public void ConnectWallet()
		{
			_beaconClientManager.Connect();
		}

		public string GetWalletAddress()
		{
			return _beaconClientManager.GetActiveWalletAddress();
		}

		public void DisconnectWallet()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestOperation(WalletOperationRequest operationRequest)
		{
			// Adjust the method to accept the WalletOperationRequest parameter
			TezosLogger.LogDebug("RequestOperation");

			Application.OpenURL("tezos://");
			await _operationRequestHandler.RequestTezosOperation(operationRequest.Destination, operationRequest.EntryPoint, operationRequest.Arg, operationRequest.Amount,
				_beaconClientManager.BeaconDappClient);
		}

		public async void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			TezosLogger.LogDebug("RequestContractOrigination - BeaconDotNet");

			await _operationRequestHandler.RequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress, _beaconClientManager.BeaconDappClient);
		}

		public async void RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			await _beaconClientManager.BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signRequest.SigningType, signRequest.Payload), signRequest.SigningType);
		}

		private void OnWalletDisconnected(WalletInfo obj)
		{
			PairingRequestData = null;
		}

		private void OnPairingRequested(PairingRequestData obj)
		{
			if (PairingRequestData != null)
			{
				return;
			}

			TezosLogger.LogDebug("WalletProvider.OnHandshakeReceived");

			PairingRequestData = obj;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
			PairWithWallet();
#endif
		}

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		private void PairWithWallet()
		{
			TezosLogger.LogDebug("Pairing with wallet...");

			UnityMainThreadDispatcher.Enqueue(() =>
			{
				var url = $"tezos://?type=tzip10&data={PairingRequestData.Data}";
				TezosLogger.LogDebug("Opening URL: " + url);
				Application.OpenURL(url);
			});
		}
#endif

		/// <summary>
		///     Triggered when a message/operation is sent to the wallet.
		///     We simply forward the event to any listeners.
		/// </summary>
		private void OnBeaconMessageSent(BeaconMessageType beaconMessageType)
		{
			switch (beaconMessageType)
			{
				case BeaconMessageType.permission_request:
					OperationRequested?.Invoke(WalletMessageType.ConnectionRequest);
					break;
				case BeaconMessageType.operation_request:
					OperationRequested?.Invoke(WalletMessageType.OperationRequest);
					break;
				case BeaconMessageType.sign_payload_request:
					OperationRequested?.Invoke(WalletMessageType.SignPayloadRequest);
					break;
				case BeaconMessageType.disconnect:
					OperationRequested?.Invoke(WalletMessageType.DisconnectionRequest);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(beaconMessageType), beaconMessageType, null);
			}
		}
	}

}