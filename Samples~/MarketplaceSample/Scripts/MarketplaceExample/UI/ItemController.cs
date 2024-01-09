using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class ItemController : MonoBehaviour
	{
		[SerializeField] private TMP_InputField transferAddressInput;
		[SerializeField] private TMP_InputField amountInput;
		[SerializeField] private TMP_InputField priceInput;
		[SerializeField] private Button transferButton;
		[SerializeField] private Button addToMarketButton;
		[SerializeField] private Button removeFromMarketButton;

		private bool _hasMoreThanOneItem;
		private IItemModel item;

		public void AddItemToMarket()
		{
			var price = int.Parse(priceInput.text);
			ExampleFactory.Instance.GetExampleManager().AddItemToMarket(item.ID, price);
		}

		public void OnIsItemOnMarket(bool success)
		{
			addToMarketButton.gameObject.SetActive(!success);
			removeFromMarketButton.gameObject.SetActive(success);
			transferButton.gameObject.SetActive(!success);
			priceInput.interactable = !success;
		}

		public void RemoveItemFromMarket()
		{
			ExampleFactory.Instance.GetExampleManager().RemoveItemFromMarket(item.ID);
		}

		public void SetItem(IItemModel newItem)
		{
			item = newItem;

			// is the item already on the market or not
			ExampleFactory.Instance.GetExampleManager().IsItemOnMarket(item.ID, item.Owner, OnIsItemOnMarket);

			// check the amount of the item (using the price)
			_hasMoreThanOneItem = item.Price > 1;
			amountInput.gameObject.SetActive(_hasMoreThanOneItem);

			// reset input fields
			transferAddressInput.text = "";
			amountInput.text = "";
			priceInput.text = "";
		}

		public void TransferItem()
		{
			var destAddress = transferAddressInput.text;
			var amountString = amountInput.text;
			var amount = 1;

			if (_hasMoreThanOneItem)
			{
				amount = amountString == string.Empty ? 1 : int.Parse(amountString);
			}

			ExampleFactory.Instance.GetExampleManager().TransferItem(item.ID, amount, destAddress);
		}
	}

}