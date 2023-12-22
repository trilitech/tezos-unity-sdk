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
		private readonly EventDispatcher _eventDispatcher;
		private readonly OperationRequestHandler _operationRequestHandler;

		public BeaconConnectorDotNet(
			WalletEventManager eventManager,
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata dAppMetadata)
		{
			var walletProviderInfo = new WalletProviderInfo
			{
				Network = network,
				Rpc = rpc,
				WalletProviderType = walletProviderType,
				Metadata = dAppMetadata
			};

			_eventDispatcher = new EventDispatcher(eventManager);
			_beaconClientManager = new BeaconClientManager(_eventDispatcher, walletProviderInfo);
			_operationRequestHandler = new OperationRequestHandler(walletProviderInfo);
		}

		private DappBeaconClient BeaconDappClient
		{
			get => _beaconClientManager.BeaconDappClient;
		}

		public void ConnectAccount()
		{
			_beaconClientManager.ConnectAccount();
		}

		public string GetActiveAccountAddress()
		{
			return _beaconClientManager.GetActiveAccountAddress();
		}

		public void DisconnectAccount()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
			await _operationRequestHandler.RequestTezosPermission(networkName, networkRPC, BeaconDappClient);
		}

		public async void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0,
			string networkName = "",
			string networkRPC = "")
		{
			await _operationRequestHandler.RequestTezosOperation(destination, entryPoint, input, amount, networkName,
				networkRPC, BeaconDappClient);
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
			_beaconClientManager.InitWalletProvider(network, rpc, walletProviderType, metadata);
		}

		public void OnReady()
		{
		}

		public void Dispose()
		{
			BeaconDappClient.Disconnect();
		}
	}

	// todo: this logger didn't work inside Beacon, improve this.

}