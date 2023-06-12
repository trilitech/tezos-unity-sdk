using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tezos.StarterSample
{
    public struct NFTData
    {
        public string id { get; set; }
        public string amount { get; set; }
        public ContractItem item { get; set; }
    }
    
    public struct ContractItem
    {
        public string damage { get; set; }
        public string armor { get; set; }
        public string attackSpeed { get; set; }
        public string healthPoints { get; set; }
        public string manaPoints { get; set; }
        public string itemType { get; set; }
    }
    
    public class UINFTElement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _nftImage;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private List<Sprite> _items;
        
        public void InitNFT(NFTData nftData)
        {
            //_nftImage.sprite = nftData.sprite;
            _idText.text = "ID: " + nftData.id;
            _amountText.text = "Amount: " + nftData.amount;
            Debug.Log(nftData.item);
            _nftImage.sprite = _items[Convert.ToInt32(nftData.id) == 0 ? 0 : Convert.ToInt32(nftData.item.itemType) % 4 + 1];
        }
    }
}