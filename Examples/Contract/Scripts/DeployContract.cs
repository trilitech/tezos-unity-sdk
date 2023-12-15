#region

using TezosSDK.Common.Scripts;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

#endregion

namespace TezosSDK.Contract.Scripts
{

	public class DeployContract : MonoBehaviour
	{
		[SerializeField] private TMP_InputField contractAddressText;
		[SerializeField] private TextMeshProUGUI tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		public void HandleDeploy()
		{
			TezosManager.Instance.Tezos.TokenContract.Deploy(OnContractDeployed);
		}

		private void OnContractDeployed(string contractAddress)
		{
			contractInfoUI.SetAddress(contractAddress);
			tokensCountText.text = "0";
		}
	}

}