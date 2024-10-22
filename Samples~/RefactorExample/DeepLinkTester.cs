using System.Reflection;
using UnityEngine;

namespace Samples.RefactorExample
{
	public class DeepLinkTester : MonoBehaviour
	{
		public string deepLinkURL = "";

		[ContextMenu("Trigger Deep Link")]
		public void SimulateDeepLinkActivation()
		{
			// Use reflection to invoke the internal InvokeDeepLinkActivated method
			var methodInfo = typeof(Application).GetMethod("InvokeDeepLinkActivated", BindingFlags.NonPublic | BindingFlags.Static);

			methodInfo?.Invoke(null, new object[]
			                         {
				                         deepLinkURL
			                         });
		}
	}
}