using System;
using UnityEngine;

namespace Tezos.WalletProvider
{
	public class WebGLEventBridge : MonoBehaviour
	{
		public event Action<string> EventReceived;
		
		private void HandleEvent(string data) => EventReceived?.Invoke(data);
	}
}