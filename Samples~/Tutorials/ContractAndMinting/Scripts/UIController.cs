using Tezos.API;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.ContractAndMinting
{

	public class UIController : MonoBehaviour
	{
		[SerializeField] private GameObject buttons;
		[SerializeField] private GameObject tokensCount;

		private void Start()
		{
			tokensCount.SetActive(false);
			buttons.SetActive(false);

			TezosAPI.WalletConnected += _ =>
			{
				tokensCount.SetActive(true);
				buttons.SetActive(true);
			};

			TezosAPI.WalletDisconnected += () =>
			{
				tokensCount.SetActive(false);
				buttons.SetActive(false);
			};
		}
	}

}