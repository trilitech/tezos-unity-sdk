using TezosSDK.Beacon;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Common.Scripts
{
    public class ContractInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField addressText;

        private readonly string _notConnectedText = "Not connected";

        private void Start()
        {
            // Subscribe to wallet events
            TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
            TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
        }

        private void OnAccountConnected(AccountInfo accountInfo)
        {
            var contractAddress = TezosManager.Instance.Tezos.TokenContract.Address;
            addressText.text = string.IsNullOrEmpty(contractAddress) ? "Not deployed" : contractAddress;
            UpdateLayout(); // Update layout to fit the new text
        }

        private void OnAccountDisconnected(AccountInfo accountInfo)
        {
            addressText.text = _notConnectedText;
            UpdateLayout(); // Update layout to fit the new text
        }

        private void UpdateLayout()
        {
            var layoutGroup = GetComponent<HorizontalLayoutGroup>();

            if (layoutGroup != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.GetComponent<RectTransform>());
            }
        }

        public void SetAddress(string address)
        {
            addressText.text = address;
        }
    }
}