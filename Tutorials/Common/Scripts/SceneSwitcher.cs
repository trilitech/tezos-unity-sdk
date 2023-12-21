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
			SceneManager.LoadScene(sceneNameToLoad);
		}
	}

}