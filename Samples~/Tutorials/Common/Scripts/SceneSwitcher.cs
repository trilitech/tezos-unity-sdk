using TezosSDK.Tezos;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TezosSDK.Tutorials.Common
{

	public class SceneSwitcher : MonoBehaviour
	{
		[SerializeField] private string sceneNameToLoad;

		public void ChangeToScene()
		{
			if (TezosManager.Instance)
			{
				TezosManager.Instance.Wallet.Disconnect();
			}

			SceneManager.LoadScene(sceneNameToLoad);
		}
	}

}