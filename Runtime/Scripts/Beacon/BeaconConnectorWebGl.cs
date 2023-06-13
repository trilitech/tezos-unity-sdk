#if UNITY_WEBGL

using System.Runtime.InteropServices;
using Beacon.Sdk.Beacon.Sign;
using TezosSDK.Tezos.Wallet;

namespace TezosSDK.Beacon
{
    /// <summary>
    /// WebGL implementation of the BeaconConnector.
    /// Binds the functions implemented inside the file BeaconConnection.jslib
    /// </summary>
    public class BeaconConnectorWebGl : IBeaconConnector
    {
        #region Bridge to external functions
        
        [DllImport("__Internal")]
        private static extern void JsInitWallet(string network, string rpc, string walletProvider);

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

        #endregion

        private string _activeAccountAddress;

        public void InitWalletProvider(string network, string rpc, WalletProviderType walletProviderType)
        {
            JsInitWallet(network, rpc, walletProviderType.ToString());
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

        public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
            ulong amount = 0, string networkName = "", string networkRPC = "")
        {
            JsSendContractCall(destination, amount.ToString(), entryPoint, arg);
        }

        public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
        {
            JsSignPayload((int)signingType, payload);
        }
    }
}
#endif