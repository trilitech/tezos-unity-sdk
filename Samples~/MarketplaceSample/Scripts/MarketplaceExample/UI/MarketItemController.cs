using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using UnityEngine;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class MarketItemController : MonoBehaviour
	{
		private IItemModel item;

		public void BuyItemFromMarket()
		{
			if (item != null)
			{
				ExampleFactory.Instance.GetExampleManager().BuyItem(item.Owner, item.ID);
			}
			else
			{
				Debug.LogError("No item selected!");
			}
		}

		public void OnItemBoughtFromMarket(bool success)
		{
			if (success)
			{
				Debug.Log("Item removed from market!");
			}
			else
			{
				Debug.Log("Failed to remove item from market.");
			}
		}

		public void SetItem(IItemModel newItem)
		{
			item = newItem;
		}
	}

}