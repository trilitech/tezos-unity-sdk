using System.Collections;
using System.Text.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public class TestMintNFT : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _textTxnHash;
        [SerializeField] private Button _hyperlinkButton;
        [Header("Properties")]
        [SerializeField] private string _contractAddress = "KT1WguzxyLmuKbJhz3jNuoRzzaUCncfp6PFE";
        
        private void OnEnable()
        {
            _button.onClick.AddListener(OnMintNFTButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnMintNFTButtonClicked);
        }

        public void ChangeNFTContractAddress(string newAddress)
        {
            _contractAddress = newAddress;
        }
        
        private void OnMintNFTButtonClicked()
        {
            _button.interactable = true;
            _textTxnHash.text = "Requested.";
            _hyperlinkButton.interactable = false;
            
            string entrypoint = "mint";
            string input = "{\"prim\": \"Unit\"}";

            StarterTezosManager.Instance.MessageReceiver.ContractCallInjected += OnContractCallInjected;
            StarterTezosManager.Instance.CallContract(_contractAddress, entrypoint, input, 0);
        }

        private void OnContractCallInjected(string transaction)
        {
            StarterTezosManager.Instance.MessageReceiver.ContractCallInjected -= OnContractCallInjected;
            var json = JsonSerializer.Deserialize<JsonElement>(transaction);
            var transactionHash = json.GetProperty("transactionHash").GetString();
            IEnumerator routine = StarterTezosManager.Instance.TrackTransaction(transactionHash, result =>
            {
                if (result.success)
                {
                    _textTxnHash.text = result.transactionHash;
                    _hyperlinkButton.interactable = true;
                }
                else
                {
                    _textTxnHash.text = "Failed.";
                }
            });
            StarterTezosManager.Instance.StartCoroutine(routine);
            _textTxnHash.text = "Pending...";
        }
    }
}