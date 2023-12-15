using System.Linq;
using TezosSDK.Beacon;
using TezosSDK.Common.Scripts;
using TezosSDK.Tezos;
using TezosSDK.Tezos.API.Models.Filters;
using TMPro;
using UnityEngine;

namespace TezosSDK.Transfer.Scripts
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject transferControls;
        [SerializeField] private TextMeshProUGUI tokenIdsText;
        [SerializeField] private ContractInfoUI contractInfoUI;

        private void Start()
        {
            // Subscribe to account connection events
            TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
            TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;

            transferControls.SetActive(false);
        }

        private void OnAccountDisconnected(AccountInfo _)
        {
            transferControls.SetActive(false);
        }

        private void OnAccountConnected(AccountInfo _)
        {
            transferControls.SetActive(true);

            var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;

            if (!string.IsNullOrEmpty(contractAddress))
            {
                GetContractTokenIds(contractAddress);
                return;
            }

            var getOriginatedContractsRoutine = TezosManager
                .Instance
                .Tezos
                .GetOriginatedContracts(contracts =>
                {
                    var tokenContracts = contracts.ToList();
                    if (!tokenContracts.Any())
                    {
                        var activeAddress = TezosManager
                            .Instance
                            .Tezos
                            .Wallet
                            .GetActiveAddress();

                        tokenIdsText.text = $"{activeAddress} didn't deploy any contract yet.";
                        return;
                    }

                    var initializedContract = tokenContracts.First();
                    TezosManager
                        .Instance
                        .Tezos
                        .TokenContract = initializedContract;

                    contractInfoUI.SetAddress(initializedContract.Address);
                    GetContractTokenIds(initializedContract.Address);
                });

            StartCoroutine(getOriginatedContractsRoutine);
        }

        private void GetContractTokenIds(string contractAddress)
        {
            var tokensForContractCoroutine = TezosManager
                .Instance
                .Tezos
                .API
                .GetTokensForContract(
                    callback: tokens =>
                    {
                        var idsResult = tokens
                            .Aggregate(string.Empty, (resultString, token) => $"{resultString}{token.TokenId}, ");
                        tokenIdsText.text = idsResult[..^2];
                    },
                    contractAddress: contractAddress,
                    withMetadata: false,
                    maxItems: 10_000,
                    orderBy: new TokensForContractOrder.Default(0));

            StartCoroutine(tokensForContractCoroutine);
        }
    }
}