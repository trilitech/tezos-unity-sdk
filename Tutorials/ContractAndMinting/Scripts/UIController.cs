#region

using TezosSDK.Tezos;
using UnityEngine;

#endregion

namespace TezosSDK.Tutorials.ContractAndMinting.Scripts
{

	public class UIController : MonoBehaviour
	{
		[SerializeField] private GameObject buttons;
		[SerializeField] private GameObject tokensCount;

		private void Start()
		{
			tokensCount.SetActive(false);
			buttons.SetActive(false);

			var messageReceiver = TezosManager.Instance.EventManager;

			messageReceiver.WalletConnected += _ =>
			{
				tokensCount.SetActive(true);
				buttons.SetActive(true);
			};

			messageReceiver.WalletDisconnected += _ =>
			{
				tokensCount.SetActive(false);
				buttons.SetActive(false);
			};
		}
	}

}