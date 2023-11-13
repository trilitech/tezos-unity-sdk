using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Contract.Scripts
{
    public class DeployContract : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI contractAddressText;
        [SerializeField] private TextMeshProUGUI tokensCountText;

        public void HandleDeploy()
        {
            TezosManager
                .Instance
                .Tezos
                .TokenContract
                .Deploy(OnContractDeployed);
        }

        private void OnContractDeployed(string contractAddress)
        {
            contractAddressText.text = contractAddress;
            tokensCountText.text = "0";
        }
    }
}