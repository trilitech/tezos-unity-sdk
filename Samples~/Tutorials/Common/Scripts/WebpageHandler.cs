using UnityEngine;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class WebpageHandler : MonoBehaviour
	{
		[SerializeField] private string webpage;

		public void OpenWebpage()
		{
			Application.OpenURL(webpage);
		}
	}

}