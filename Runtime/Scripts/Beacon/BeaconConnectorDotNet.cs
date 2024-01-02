#region

using System;
using Beacon.Sdk.Beacon.Sign;
using Beacon.Sdk.BeaconClients;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;

#endregion

namespace TezosSDK.Beacon
{

	public class BeaconConnectorDotNet : IBeaconConnector, IDisposable
	{
		private readonly BeaconClientManager _beaconClientManager;
		private readonly OperationRequestHandler _operationRequestHandler;
		private readonly EventDispatcher _eventDispatcher;
		private readonly WalletProviderInfo _walletProviderInfo;

		public BeaconConnectorDotNet(
			WalletEventManager eventManager,
			string network,
			string rpc,
			DAppMetadata dAppMetadata)
		{

			_walletProviderInfo = new WalletProviderInfo
			{
				Network = network,
				Rpc = rpc,
				Metadata = dAppMetadata
			};

			_eventDispatcher = new EventDispatcher(eventManager);
			_beaconClientManager = new BeaconClientManager(_eventDispatcher);
			_operationRequestHandler = new OperationRequestHandler(_walletProviderInfo);
			
			_beaconClientManager.SetWalletProviderInfo(_walletProviderInfo);
			_beaconClientManager.Create();
			_beaconClientManager.InitAsync();
			
			eventManager.HandshakeReceived += OnHandshakeReceived;
		}

		private void OnHandshakeReceived(HandshakeData obj)
		{
			_beaconClientManager.ConnectDappClient();
		}

		public BeaconConnectorDotNet(WalletEventManager eventManager)
		{
			_eventDispatcher = new EventDispatcher(eventManager);
		}

		private DappBeaconClient BeaconDappClient
		{
			get => _beaconClientManager.BeaconDappClient;
		}

		public void ConnectAccount()
		{
			_beaconClientManager.ConnectDappClient();
		}

		public string GetActiveAccountAddress()
		{
			return _beaconClientManager.GetActiveAccountAddress();
		}

		public void DisconnectAccount()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestTezosPermission(string networkName = "")
		{
			await _operationRequestHandler.RequestTezosPermission(networkName, BeaconDappClient);
		}

		public async void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0)
		{
			await _operationRequestHandler.RequestTezosOperation(destination, entryPoint, input, amount, BeaconDappClient);
		}

		public async void RequestContractOrigination(string script, string delegateAddress)
		{
			await _operationRequestHandler.RequestContractOrigination(script, delegateAddress, BeaconDappClient);
		}

		public async void RequestTezosSignPayload(SignPayloadType signingType, string payload)
		{
			await BeaconDappClient.RequestSign(NetezosExtensions.GetPayloadString(signingType, payload), signingType);
		}

		public void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata metadata)
		{
			// _beaconClientManager.SetWalletProviderInfo(network, rpc, walletProviderType, metadata);
		}

		public void OnReady()
		{
		}

		public void Dispose()
		{
			BeaconDappClient.Disconnect();
		}
	}
	
}