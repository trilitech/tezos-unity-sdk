using TezosSDK.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class SceneSwitcher : MonoBehaviour
	{
		[SerializeField] private string sceneNameToLoad;

		public void ChangeToScene()
		{
			TezosAPI.DisconnectWallet();

			SceneManager.LoadScene(sceneNameToLoad);
		}
	}

}