using System;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK
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