#region

using System.Runtime.InteropServices;
using Beacon.Sdk.Beacon.Permission;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TezosSDK.Tezos.Wallet;

#endregion

namespace TezosSDK.Beacon
{

	/// <summary>
	///     WebGL implementation of the BeaconConnector.
	/// </summary>
	public class BeaconConnectorWebGl : IBeaconConnector
	{
		private string _activeWalletAddress;

		public void ConnectWallet(WalletProviderType? walletProviderType)
		{
			if (walletProviderType == null)
			{
				Logger.LogError("WalletProviderType is null");
				return;
			}
			
			JsInitWallet(TezosManager.Instance.Config.Network.ToString(), TezosManager.Instance.Config.Rpc, walletProviderType.ToString(),
				TezosManager.Instance.DAppMetadata.Name, TezosManager.Instance.DAppMetadata.Url, TezosManager.Instance.DAppMetadata.Icon);
			JsConnectAccount();
		}

		public void DisconnectWallet()
		{
			JsDisconnectAccount();
		}

		public string GetWalletAddress()
		{
			return JsGetActiveAccountAddress();
		}

		public void RequestWalletConnection()
		{
		}

		public void RequestOperation(
			string destination,
			string entryPoint = "default",
			string input = null,
			ulong amount = 0)
		{
			JsSendContractCall(destination, amount.ToString(), entryPoint, input);
		}

		public void RequestSignPayload(SignPayloadType signingType, string payload)
		{
			JsSignPayload((int)signingType, payload);
		}

		public void RequestContractOrigination(string script, string delegateAddress = null)
		{
			JsRequestContractOrigination(script, delegateAddress);
		}

#if UNITY_WEBGL

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

#else

		#region Stub functions

		private void JsRequestContractOrigination(string script, string delegateAddress)
		{
		}

		private void JsInitWallet(string network, string rpc, string toString, string metadataName, string metadataUrl, string metadataIcon)
		{
		}

		private void JsUnityReadyEvent()
		{
		}

		private void JsConnectAccount()
		{
		}

		private void JsDisconnectAccount()
		{
		}

		private void JsSendContractCall(string destination, string toString, string entryPoint, string input)
		{
		}

		private string JsGetActiveAccountAddress()
		{
			return "";
		}

		private void JsSignPayload(int signingType, string payload)
		{
		}

		#endregion

#endif
	}

}