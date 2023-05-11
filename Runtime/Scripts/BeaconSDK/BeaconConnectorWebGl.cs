#if UNITY_WEBGL
using System;
using System.Runtime.InteropServices;
using Beacon.Sdk.Beacon.Sign;
using UnityEngine;

namespace BeaconSDK
{
    /// <summary>
    /// WebGL implementation of the BeaconConnector.
    /// Binds the functions implemented inside the file BeaconConnection.jslib
    /// </summary>
    public class BeaconConnectorWebGl : IBeaconConnector
    {
        #region Bridge to external functions

        [DllImport("__Internal")]
        private static extern void JsSetNetwork(string network, string rpc);

        [DllImport("__Internal")]
        private static extern void JsConnectAccount();

        [DllImport("__Internal")]
        private static extern void JsSwitchAccounts();

        [DllImport("__Internal")]
        private static extern void JsRemovePeer();

        [DllImport("__Internal")]
        private static extern void JsSendMutezAsString(string amount, string address);

        [DllImport("__Internal")]
        private static extern void JsSendContractCall(string destination, string amount, string entryPoint, string arg);

        [DllImport("__Internal")]
        private static extern void JsReset();

        [DllImport("__Internal")]
        private static extern void JsSignPayload(int signingType, string payload);

        [DllImport("__Internal")]
        private static extern string JsGetActiveAccountAddress();

        #endregion

        private string _activeAccountAddress;

        public void SetNetwork(string network, string rpc)
        {
            JsSetNetwork(network, rpc);
        }

        public void ConnectAccount()
        {
            JsConnectAccount();
        }

        public void CallContract(string destination, string entryPoint, string arg, long amount = 0)
        {
            JsSendContractCall(destination, amount.ToString(), entryPoint, arg);
        }

        public void DisconnectAccount()
        {
            JsRemovePeer();
        }

/*
		public void SendMutez(long amount, string address)
		{
			JsSendMutezAsString(amount.ToString(), address);
		}
*/
        public void SwitchAccounts()
        {
            JsSwitchAccounts();
        }

        public void Reset()
        {
            JsReset();
        }

        public string GetActiveAccountAddress()
        {
            return JsGetActiveAccountAddress();
        }

        public void RequestHandshake()
        {
            throw new NotImplementedException();
        }

        public void SetActiveAccountAddress(string address)
        {
            _activeAccountAddress = address;
        }

        public void RequestTezosPermission(string networkName = "", string networkRPC = "")
        {
            Debug.Log("ConnectAccount executes RequestPermissions");
            throw new NotImplementedException();
        }

        public void RequestTezosOperation(string destination, string entryPoint = "default", string arg = null,
            ulong amount = 0, string networkName = "", string networkRPC = "")
        {
            JsSetNetwork(networkName, networkRPC);
            JsSendContractCall(destination, amount.ToString(), entryPoint, arg);
        }

        public void RequestTezosSignPayload(SignPayloadType signingType, string payload)
        {
            JsSignPayload((int)signingType, payload);
        }

        public void RequestTezosBroadcast(string signedTransaction, string networkName = "", string networkRPC = "")
        {
            throw new NotImplementedException();
        }
    }
}
#endif