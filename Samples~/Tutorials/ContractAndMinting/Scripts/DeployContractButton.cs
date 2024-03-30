using TezosSDK.Tezos;
using TezosSDK.Tutorials.Common;
using TMPro;
using UnityEngine;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.Tutorials.ContractAndMinting
{

	public class DeployContractButton : MonoBehaviour
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
			Logger.LogDebug($"DeployContractButton - Contract deployed at address: {contractAddress}");
			contractInfoUI.SetAddress(contractAddress);
			tokensCountText.text = "0";
		}
	}

}