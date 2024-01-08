using System;
using Beacon.Sdk.Beacon;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Helpers;
using TezosSDK.Helpers.Extensions;
using TezosSDK.Tezos;

namespace TezosSDK.Beacon
{

	public class BeaconConnectorDotNet : IBeaconConnector, IDisposable
	{
		private readonly BeaconClientManager _beaconClientManager;
		private readonly OperationRequestHandler _operationRequestHandler;

		public BeaconConnectorDotNet(WalletEventManager eventManager)
		{
			_beaconClientManager = new BeaconClientManager(eventManager);
			_operationRequestHandler = new OperationRequestHandler();
			_operationRequestHandler.MessageSent += OnBeaconMessageSent;
			_beaconClientManager.Create();
		}

		public event Action<BeaconMessageType> OperationRequested;

		public void ConnectWallet(WalletProviderType? _)
		{
			_beaconClientManager.InitAsyncAndConnect();
		}

		public string GetWalletAddress()
		{
			return _beaconClientManager.GetActiveWalletAddress();
		}

		public void DisconnectWallet()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestWalletConnection()
		{
			Logger.LogDebug("RequestWalletConnection");
			await _operationRequestHandler.RequestTezosPermission(_beaconClientManager.BeaconDappClient);
		}

		public async void RequestOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0)
		{
			Logger.LogDebug("RequestOperation");

			await _operationRequestHandler.RequestTezosOperation(destination, entryPoint, input, amount,
				_beaconClientManager.BeaconDappClient);
		}

		public async void RequestContractOrigination(string script, string delegateAddress)
		{
			Logger.LogDebug("RequestContractOrigination");

			await _operationRequestHandler.RequestContractOrigination(script, delegateAddress,
				_beaconClientManager.BeaconDappClient);
		}

		public async void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			await _beaconClientManager.BeaconDappClient.RequestSign(
				NetezosExtensions.GetPayloadString(signingType, payload), signingType);
		}

		public void Dispose()
		{
			_beaconClientManager.BeaconDappClient.Disconnect();
		}

		/// <summary>
		///     Triggered when a message/operation is sent to the wallet.
		///     We simply forward the event to any listeners.
		/// </summary>
		private void OnBeaconMessageSent(BeaconMessageType beaconMessageType)
		{
			OperationRequested?.Invoke(beaconMessageType);
		}
	}

}