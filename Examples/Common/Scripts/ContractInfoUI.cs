using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;

namespace TezosSDK.Contract.Scripts
{
    public class ContractInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI addressText;
        private const string NotConnectedText = "Not connected";

        private void Start()
        {
            TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
            TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
            addressText.text = NotConnectedText;
        }

        private void OnAccountDisconnected(AccountInfo account_info)
        {
            addressText.text = NotConnectedText;
        }

        private void OnAccountConnected(AccountInfo account_info)
        {
            addressText.text = TezosManager
                .Instance
                .Tezos
                .TokenContract
                .Address;
        }
    }
}