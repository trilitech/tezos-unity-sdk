#if UNITY_WEBGL

#region

using System.Runtime.InteropServices;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;

#endregion

namespace TezosSDK.Beacon
{

	/// <summary>
	///     WebGL implementation of the BeaconConnector.
	///     Binds the functions implemented inside the file BeaconConnection.jslib
	/// </summary>
	public class BeaconConnectorWebGl : IBeaconConnector
	{
		private string _activeAccountAddress;

		public void InitWalletProvider(
			string network,
			string rpc,
			WalletProviderType walletProviderType,
			DAppMetadata metadata)
		{
			JsInitWallet(network, rpc, walletProviderType.ToString(), metadata.Name, metadata.Url, metadata.Icon);
		}

		public void OnReady()
		{
			JsUnityReadyEvent();
		}

		public void ConnectAccount()
		{
			JsConnectAccount();
		}

		public void DisconnectAccount()
		{
			JsDisconnectAccount();
		}

		public string GetActiveAccountAddress()
		{
			return JsGetActiveAccountAddress();
		}

		public void RequestTezosPermission(string networkName = "", string networkRPC = "")
		{
		}

		public void RequestTezosOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0,
			string networkName = "",
			string networkRPC = "")
		{
			JsSendContractCall(destination, amount.ToString(), entryPoint, input);
		}

		public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
		{
			JsSignPayload((int)signingType, payload);
		}

		public void RequestContractOrigination(string script, string delegateAddress = null)
		{
			JsRequestContractOrigination(script, delegateAddress);
		}

		#region Bridge to external functions

		[DllImport("__Internal")]
		private static extern void JsInitWallet(
			string network,
			string rpc,
			string walletProvider,
			string appName,
			string appUrl,
			string iconUrl);

		[DllImport("__Internal")]
		private static extern void JsConnectAccount();

		[DllImport("__Internal")]
		private static extern void JsDisconnectAccount();

		[DllImport("__Internal")]
		private static extern void JsSendContractCall(string destination, string amount, string entryPoint, string arg);

		[DllImport("__Internal")]
		private static extern void JsSignPayload(int signingType, string payload);

		[DllImport("__Internal")]
		private static extern string JsGetActiveAccountAddress();

		[DllImport("__Internal")]
		private static extern string JsRequestContractOrigination(string script, string delegateAddress);

		[DllImport("__Internal")]
		private static extern string JsUnityReadyEvent();

		#endregion
	}

}
#endif