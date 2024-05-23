using TezosSDK.Helpers.Logging;
using TezosSDK.Samples.Tutorials.Common;
using TezosSDK.Tezos.Managers;
using TMPro;
using UnityEngine;

namespace TezosSDK.Samples.Tutorials.ContractAndMinting
{

	public class DeployContractButton : MonoBehaviour
	{
		[SerializeField] private TMP_InputField tokensCountText;
		[SerializeField] private ContractInfoUI contractInfoUI;

		public void HandleDeploy()
		{
			// Deploy the contract
			// The contract will be deployed to the active address
			TezosLog.Debug("DeployContractButton - Deploying contract...");
			TezosManager.Instance.Tezos.TokenContract.Deploy(OnContractDeployed);
		}

		private void OnContractDeployed(string contractAddress)
		{
			TezosLog.Debug($"DeployContractButton - Contract deployed at address: {contractAddress}");
			contractInfoUI.SetAddress(contractAddress);
			tokensCountText.text = "0";
		}
	}

}