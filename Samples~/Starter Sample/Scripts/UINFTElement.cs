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
    }
    
    public class UINFTElement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _nftImage;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _amountText;
        
        public void InitNFT(NFTData nftData)
        {
            //_nftImage.sprite = nftData.sprite;
            _idText.text = "ID: " + nftData.id;
            _amountText.text = "Amount: " + nftData.amount;
        }
    }
}