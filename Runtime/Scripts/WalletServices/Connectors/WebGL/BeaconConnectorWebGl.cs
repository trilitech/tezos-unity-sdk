using System;
using System.Runtime.InteropServices;
using TezosSDK.Helpers.Logging;
using TezosSDK.Tezos.Interfaces.Wallet;
using TezosSDK.Tezos.Managers;
using TezosSDK.Tezos.Models;
using TezosSDK.Tezos.Wallet;
using TezosSDK.WalletServices.Data;
using TezosSDK.WalletServices.Enums;

namespace TezosSDK.WalletServices.Connectors.WebGL
{

	/// <summary>
	///     WebGL implementation of the BeaconConnector.
	/// </summary>
	public class BeaconConnectorWebGl : IWalletConnector
	{
		public BeaconConnectorWebGl(WalletEventManager walletEventManager)
		{
			walletEventManager.SDKInitialized += UnityReady;
		}

		public event Action<WalletMessageType> OperationRequested;

		public void ConnectWallet()
		{
			WalletProviderType walletProviderType = WalletProviderType.beacon; // TODO: Fix this

			if (walletProviderType == null)
			{
				TezosLog.Error("WalletProviderType is null");
				return;
			}

			JsInitWallet(TezosManager.Instance.Config.Network.ToString(), TezosManager.Instance.Config.Rpc,
				walletProviderType.ToString(), TezosManager.Instance.DAppMetadata.Name,
				TezosManager.Instance.DAppMetadata.Url, TezosManager.Instance.DAppMetadata.Icon);

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

		public void RequestOperation(WalletOperationRequest operationRequest)
		{
			JsSendContractCall(operationRequest.Destination, operationRequest.Amount.ToString(),
				operationRequest.EntryPoint, operationRequest.Arg);

			OperationRequested?.Invoke(WalletMessageType.OperationRequest);
		}

		public void RequestSignPayload(WalletSignPayloadRequest signRequest)
		{
			// Adjust the method to accept the WalletSignPayloadRequest parameter
			JsSignPayload((int)signRequest.SigningType, signRequest.Payload);
			OperationRequested?.Invoke(WalletMessageType.SignPayloadRequest);
		}

		public void RequestContractOrigination(WalletOriginateContractRequest originationRequest)
		{
			JsRequestContractOrigination(originationRequest.Script, originationRequest.DelegateAddress);
			OperationRequested?.Invoke(WalletMessageType.OperationRequest);
		}

		private void UnityReady()
		{
			JsUnityReadyEvent();
		}

#if UNITY_WEBGL
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