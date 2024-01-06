#region

using System;
using Beacon.Sdk.Beacon.Permission;
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

		public BeaconConnectorDotNet(
			WalletEventManager eventManager,
			NetworkType network,
			string rpc,
			DAppMetadata dAppMetadata)
		{
			_beaconClientManager = new BeaconClientManager(eventManager);
			_operationRequestHandler = new OperationRequestHandler();

			_beaconClientManager.Create();
		}

		public DappBeaconClient BeaconDappClient
		{
			get => _beaconClientManager.BeaconDappClient;
		}

		public void ConnectWallet()
		{
			_beaconClientManager.InitAsyncAndConnect();
		}

		public string GetActiveWalletAddress()
		{
			return _beaconClientManager.GetActiveWalletAddress();
		}

		public void DisconnectWallet()
		{
			_beaconClientManager.DisconnectWallet();
		}

		public async void RequestTezosPermission()
		{
			Logger.LogInfo("RequestTezosPermission");
			await _operationRequestHandler.RequestTezosPermission(BeaconDappClient);
		}

		public async void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0)
		{
			await _operationRequestHandler.RequestTezosOperation(destination, entryPoint, input, amount,
				BeaconDappClient);
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
			// TODO: fix or remove
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