using System;
using UnityEngine;

namespace Tezos.SocialLoginProvider
{
	public class KukaiWebGLEventBridge : MonoBehaviour
	{
		public event Action<string> EventReceived;

		private void HandleEvent(string data) => EventReceived?.Invoke(data);
	}
}