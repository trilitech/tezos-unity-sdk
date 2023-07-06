using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace TezosSDK.Samples.DemoExample
{
    public class MarketManager : MonoBehaviour
    {
        [SerializeField] private ItemView itemViewPrefab;
        [SerializeField] private ItemView itemFullDisplay;
        [SerializeField] private Transform contentParent;

        private List<ItemView> _marketItems = new List<ItemView>();
        private ItemView _currentSelectedItem;

        public void Init(List<IItemModel> items)
        {
            ClearMarket();

            foreach (var item in items)
                AddItem(item);
        }

        public void CheckSelection()
        {
            if (_currentSelectedItem == null) return;

            itemFullDisplay.DisplayItem(null, null);
        }

        private void AddItem(IItemModel itemModel)
        {
            ItemView newItem = Instantiate(itemViewPrefab, contentParent);
            newItem.DisplayItem(itemModel);
            newItem.OnItemSelected = OnItemSelected;
            _marketItems.Add(newItem);
        }

        private void ClearMarket()
        {
            _marketItems = new List<ItemView>();
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }

            CheckSelection();
        }

        private void OnItemSelected(ItemView selectedItem)
        {
            if (_currentSelectedItem != null)
                _currentSelectedItem.Unselect();

            if (_currentSelectedItem == selectedItem)
            {
                selectedItem.Unselect();
                if (itemFullDisplay != null)
                    itemFullDisplay.ClearItem();
                _currentSelectedItem = null;
                return;
            }

            selectedItem.Select();
            _currentSelectedItem = selectedItem;
            if (itemFullDisplay != null)
                itemFullDisplay.DisplayItem(selectedItem.Item, selectedItem.CachedSprite);
            itemFullDisplay.GetComponent<MarketItemController>().SetItem(selectedItem.Item);
        }
    }
}
