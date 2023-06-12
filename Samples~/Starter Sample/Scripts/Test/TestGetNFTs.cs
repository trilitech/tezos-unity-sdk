using System;
using System.Collections.Generic;
using Scripts.Helpers;
using Scripts.Tezos;
using Scripts.Tezos.API;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Tezos.StarterSample
{
    public class TestGetNFTs : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private RectTransform _trContent;
        [SerializeField] private GameObject _nftElementPrefab;
        [Header("Properties")]
        [SerializeField] private string _contractAddress = "KT1WguzxyLmuKbJhz3jNuoRzzaUCncfp6PFE";
        
        private void OnEnable()
        {
            _button.onClick.AddListener(OnGetNFTsButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnGetNFTsButtonClicked);
        }
        
        public void ChangeNFTContractAddress(string newAddress)
        {
            _contractAddress = newAddress;
        }
        
        private void OnGetNFTsButtonClicked()
        {
            _button.interactable = false;
            _resultText.text = "Pending...";

            foreach (RectTransform tr in _trContent)
            {
                Destroy(tr.gameObject);
            }

            var activeWalletAddress = StarterTezosManager.Instance.GetActiveAddress(); // Address to the current active account

            const string entrypoint = "view_items_of";
            var input = new { @string = activeWalletAddress };

            CoroutineRunner.Instance.StartWrappedCoroutine(
                StarterTezosManager.Instance.API.ReadView(
                    contractAddress: _contractAddress,
                    entrypoint: entrypoint,
                    input: input,
                    callback: result =>
                    {
                        Debug.Log(result);
                        // Deserialize the json data to inventory items
                        CoroutineRunner.Instance.StartWrappedCoroutine(
                            NetezosExtensions.HumanizeValue(
                                val: result,
                                rpcUri: TezosConfig.Instance.RpcBaseUrl,
                                destination: _contractAddress,
                                humanizeEntrypoint: "humanizeInventory",
                                onComplete: (NFTData[] inventory) =>
                                    OnInventoryFetched(inventory))
                        );
                        
                        _button.interactable = true;
                        _resultText.text = "Fetched.";
                    }));
        }
        
        private void OnInventoryFetched(NFTData[] inventory)
        {
            if (inventory != null)
            {
                foreach (var nftData in inventory)
                {
                    UINFTElement uinftElement = Instantiate(_nftElementPrefab, _trContent).GetComponent<UINFTElement>();
                    uinftElement.InitNFT(nftData);
                }
            }
        }
    }
}
