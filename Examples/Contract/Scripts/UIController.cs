using TezosSDK.Tezos;
using UnityEngine;

namespace TezosSDK.Contract.Scripts
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private GameObject buttons;
        [SerializeField] private GameObject tokensCount;

        private void Start()
        {
            tokensCount.SetActive(false);
            buttons.SetActive(false);
            
            var messageReceiver = TezosManager
                .Instance
                .MessageReceiver;

            messageReceiver.AccountConnected += _ =>
            {
                tokensCount.SetActive(true);
                buttons.SetActive(true);
            };

            messageReceiver.AccountDisconnected += _ =>
            {
                tokensCount.SetActive(false);
                buttons.SetActive(false);
            };
        }
    }
}