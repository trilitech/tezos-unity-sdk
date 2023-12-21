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
		[SerializeField] private TMP_InputField tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		public void HandleDeploy()
		{
			// Deploy the contract
			// The contract will be deployed to the active address
			TezosManager.Instance.Tezos.TokenContract.Deploy(OnContractDeployed);
		}

		private void OnContractDeployed(string contractAddress)
		{
			contractInfoUI.SetAddress(contractAddress);
			tokensCountText.text = "0";
		}
	}

}