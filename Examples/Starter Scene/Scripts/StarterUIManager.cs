using Scripts.Tezos.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class StarterUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject _connectPanel;
        [SerializeField] private GameObject _qrCodePanel;
        [SerializeField] private GameObject _testingPanel;
        [SerializeField] private GameObject _transferPanel;
        [SerializeField] private GameObject _nftPanel;
        [SerializeField] private Toggle _transferPanelToggle;
        [SerializeField] private Toggle _nftPanelToggle;
        [SerializeField] private TextMeshProUGUI _accountAddressText;

        private void Start()
        {
            _connectPanel.SetActive(true);
            _testingPanel.SetActive(false);
            _qrCodePanel.SetActive(false);
            TezosManager.Instance.OnIsConnectedChanged += OnIsConnectedChanged;
        }

        private void OnDestroy()
        {
            TezosManager.Instance.OnIsConnectedChanged -= OnIsConnectedChanged;
        }

        private void OnIsConnectedChanged(bool newValue)
        {
            _connectPanel.SetActive(!newValue);
            _qrCodePanel.SetActive(false);
            _testingPanel.SetActive(newValue);

            if (newValue)
            {
                _accountAddressText.text = TezosManager.Instance.GetActiveAddress();
                _transferPanel.SetActive(true);
                _nftPanel.SetActive(false);
                _transferPanelToggle.isOn = true;
                _nftPanelToggle.isOn = false;
            }
        }

        public void OnNativeConnectButtonClicked()
        {
            TezosManager.Instance.Connect(WalletProviderType.beacon, true);
        }

        public void OnDisconnectButtonClicked()
        {
            TezosManager.Instance.Disconnect();
        }
    }
}