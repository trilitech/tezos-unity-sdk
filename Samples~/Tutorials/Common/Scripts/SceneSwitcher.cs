using TezosSDK.Tezos.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TezosSDK.Samples.Tutorials.Common
{

	public class SceneSwitcher : MonoBehaviour
	{
		[SerializeField] private string sceneNameToLoad;

		public void ChangeToScene()
		{
			if (TezosManager.Instance)
			{
				TezosManager.Instance.WalletConnector.DisconnectWallet();
			}

			SceneManager.LoadScene(sceneNameToLoad);
		}
	}

}