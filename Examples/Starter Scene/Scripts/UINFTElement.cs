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
        
        public void InitNFT(NFTData nftData)
        {
            _idText.text = "ID: " + nftData.id;
            _amountText.text = "Amount: " + nftData.amount;
        }
    }
}