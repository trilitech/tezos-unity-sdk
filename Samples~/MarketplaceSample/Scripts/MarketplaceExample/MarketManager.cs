using System.Collections.Generic;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using TezosSDK.MarketplaceSample.MarketplaceExample.UI;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample
{

	public class MarketManager : MonoBehaviour
	{
		[SerializeField] private ItemView itemViewPrefab;
		[SerializeField] private ItemView itemFullDisplay;
		[SerializeField] private Transform contentParent;
		private ItemView _currentSelectedItem;

		private List<ItemView> _marketItems = new();

		public void CheckSelection()
		{
			if (_currentSelectedItem == null)
			{
				return;
			}

			itemFullDisplay.DisplayItem(null);
		}

		public void Init(List<IItemModel> items)
		{
			ClearMarket();

			foreach (var item in items)
			{
				AddItem(item);
			}
		}

		private void AddItem(IItemModel itemModel)
		{
			var newItem = Instantiate(itemViewPrefab, contentParent);
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
			{
				_currentSelectedItem.Unselect();
			}

			if (_currentSelectedItem == selectedItem)
			{
				selectedItem.Unselect();

				if (itemFullDisplay != null)
				{
					itemFullDisplay.ClearItem();
				}

				_currentSelectedItem = null;
				return;
			}

			selectedItem.Select();
			_currentSelectedItem = selectedItem;

			if (itemFullDisplay != null)
			{
				itemFullDisplay.DisplayItem(selectedItem.Item, selectedItem.CachedSprite);
			}

			itemFullDisplay.GetComponent<MarketItemController>().SetItem(selectedItem.Item);
		}
	}

}