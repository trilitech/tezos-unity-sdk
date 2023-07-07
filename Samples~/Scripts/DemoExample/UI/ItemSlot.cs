using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.Samples.DemoExample
{
    [System.Obsolete]
    [RequireComponent(typeof(Image))]
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private Transform _itemSpace;

        public Transform ItemSpace => _itemSpace;

        public DraggableItemVisual ItemInSlot => _itemInSlot;

        private DraggableItemVisual _itemInSlot;

        /// <summary>
        /// assigns an item to slot and lets the item know what slot it's set to
        /// </summary>
        /// <param name="item">The item to be asigned to this slot</param>
        public void SetToSlot(DraggableItemVisual item)
        {
            if (_itemInSlot != item)
            {
                _itemInSlot = item;
                _itemInSlot.SetSlot(this);
            }


            item.SetItemPlacement(this);
        }
    }
}