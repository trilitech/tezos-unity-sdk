using UnityEngine;

namespace TezosSDK.Beacon
{
	public class BeaconWebGLReceiver : MonoBehaviour
	{
		/// <summary>
		/// Method called  externally when the active account has changed
		/// </summary>
		/// <param name="address">Address of the new account</param>
		public void OnActiveAccountChanged(string address)
		{
			// ((Database)DatabaseFactory.Instance.GetDatabase()).SetBeaconAccountAddress(address);
			// Debug.Log("Account Address receievd: " + address);
		}

		/// <summary>
		/// Method called externally when the count of connected accounts has changed
		/// </summary>
		/// <param name="count">Count of active accounts</param>
		public void OnAccountsCountChanged(int count)
		{
		
		}

		public void DebugMessage(string msg)
		{
			Debug.Log("WebGL Debug Msg: " + msg);
		}
	}
}
