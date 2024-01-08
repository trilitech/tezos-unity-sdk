using UnityEngine;

namespace TezosSDK.Tutorials.Common.Scripts
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