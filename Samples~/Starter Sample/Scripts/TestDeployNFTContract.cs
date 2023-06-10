using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class TestDeployNFTContract : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _deployButton;

        [SerializeField] private TextMeshProUGUI _textTxnHash;
        [SerializeField] private Button _hyperlinkButtonTxnHash;

        [SerializeField] private TextMeshProUGUI _textContractAddress;
        [SerializeField] private Button _hyperlinkButtonContractAddress;

        [SerializeField] private TestMintNFT _testMintNFT;
        [SerializeField] private TestGetNFTs _testGetNFTs;
        
        private void OnEnable()
        {
            _deployButton.onClick.AddListener(OnDeployNFTContractButtonClicked);
        }

        private void OnDisable()
        {
            _deployButton.onClick.RemoveListener(OnDeployNFTContractButtonClicked);
        }

        private void OnDeployNFTContractButtonClicked()
        {
            _deployButton.interactable = false;

            _textTxnHash.text = "";
            _hyperlinkButtonTxnHash.interactable = false;

            _textContractAddress.text = "";
            _hyperlinkButtonContractAddress.interactable = false;
            
            // TODO:
        }
    }
}