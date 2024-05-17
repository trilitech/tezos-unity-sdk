using System;
using Beacon.Sdk.Beacon;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Beacon;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;
using TezosSDK.WalletServices.Helpers;

namespace TezosSDK.WalletServices.Connectors.DotNet
{

	public class BeaconConnectorDotNet : IWalletConnector, IDisposable
	{
		private readonly BeaconClientManager _beaconClientManager;
		private readonly OperationRequestHandler _operationRequestHandler;

		public BeaconConnectorDotNet(WalletEventManager eventManager)
		{
			_beaconClientManager = new BeaconClientManager(eventManager, this);
			_operationRequestHandler = new OperationRequestHandler();
			_operationRequestHandler.MessageSent += OnBeaconMessageSent;
			_beaconClientManager.Create();
		}

		public void Dispose()
		{
			_beaconClientManager.BeaconDappClient.Disconnect();
		}

		public event Action<WalletMessageType> OperationRequested;

		public async void ConnectWallet()
		{
			await _beaconClientManager.Initalize();
			await _operationRequestHandler.RequestTezosPermission(_beaconClientManager.BeaconDappClient);
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
			TezosLog.Debug("RequestOperation");

			await _operationRequestHandler.RequestTezosOperation(operationRequest.Destination,
				operationRequest.EntryPoint, operationRequest.Arg, operationRequest.Amount,
				_beaconClientManager.BeaconDappClient);
		}

		public async void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			TezosLog.Debug("RequestContractOrigination");

			await _operationRequestHandler.RequestContractOrigination(originationRequest.Script,
				originationRequest.DelegateAddress, _beaconClientManager.BeaconDappClient);
		}

		public async void RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			await _beaconClientManager.BeaconDappClient.RequestSign(
				NetezosExtensions.GetPayloadString(signRequest.SigningType, signRequest.Payload),
				signRequest.SigningType);
		}

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