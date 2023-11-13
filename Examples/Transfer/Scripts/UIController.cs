using System.Linq;
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

        private void Start()
        {
            transferControls.SetActive(false);

            var messageReceiver = TezosManager
                .Instance
                .MessageReceiver;

            messageReceiver.AccountConnected += _ =>
            {
                transferControls.SetActive(true);

                var contractAddress = TezosManager
                    .Instance
                    .Tezos
                    .TokenContract
                    .Address;

                if (string.IsNullOrEmpty(contractAddress)) return;

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
            };
            messageReceiver.AccountDisconnected += _ => { transferControls.SetActive(false); };
        }
    }
}