using Tezos.API;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class SceneSwitcher : MonoBehaviour
	{
		[SerializeField] private string sceneNameToLoad;

		public void ChangeToScene()
		{
			TezosAPI.Disconnect().GetAwaiter().GetResult();
			SceneManager.LoadScene(sceneNameToLoad);
		}
	}

}