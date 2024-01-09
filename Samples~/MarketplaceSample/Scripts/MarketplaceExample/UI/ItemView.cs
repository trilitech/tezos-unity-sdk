using System;
using System.Globalization;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class ItemView : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private Image backgroundImage;
		[SerializeField] private Image itemImage;
		[SerializeField] private TextMeshProUGUI priceText;
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI typeText;
		[SerializeField] private StatsView statsView;
		[SerializeField] private Color selectColor = Color.white;
		[SerializeField] private Sprite defaultImage;
		public Action<ItemView> OnItemSelected;
		private Sprite _initSprite;

		private Color _originalBackgroundColor;
		public Sprite CachedSprite { get; private set; }

		public IItemModel Item { get; private set; }

		#region IPointerClickHandler Implementation

		public void OnPointerClick(PointerEventData eventData)
		{
			OnItemSelected?.Invoke(this);
		}

		#endregion

		private void Start()
		{
			_originalBackgroundColor = backgroundImage.color;
			_initSprite = itemImage.sprite;
		}

		public void ClearItem()
		{
			if (priceText != null)
			{
				priceText.text = string.Empty;
			}

			if (nameText != null)
			{
				nameText.text = string.Empty;
			}

			if (typeText != null)
			{
				typeText.text = string.Empty;
			}

			if (itemImage != null)
			{
				itemImage.sprite = defaultImage;
			}

			if (statsView != null)
			{
				statsView.Clear();
			}

			Item = null;
		}

		public void DisplayItem(IItemModel item, Sprite cahcedSprite = null)
		{
			if (item == null)
			{
				Debug.LogWarning("item in market currently null -test message-");

				priceText.text = "Price";
				nameText.text = "Name";
				typeText.text = "[Type]";
				var statParams = new StatParams(0, 0, 0, 0, 0);
				statsView.DisplayStats(statParams);
				itemImage.sprite = _initSprite;
				return;
			}

			if (priceText != null)
			{
				priceText.text = item.Price.ToString(CultureInfo.InvariantCulture);
			}

			if (nameText != null)
			{
				nameText.text = item.Name;
			}

			if (typeText != null)
			{
				typeText.text = item.Type.ToString();
			}

			if (statsView != null)
			{
				statsView.DisplayStats(item.Stats);
			}

			if (cahcedSprite != null)
			{
				itemImage.sprite = cahcedSprite;
				CachedSprite = itemImage.sprite;
			}
			else if (itemImage != null)
			{
				var itemRes = Resources.Load<ItemReseource>(item.ResourcePath);
				itemImage.sprite = itemRes.ItemSprite;
				CachedSprite = itemImage.sprite;
			}

			Item = item;
		}

		public void Select()
		{
			backgroundImage.color = selectColor;
		}

		public void Unselect()
		{
			backgroundImage.color = _originalBackgroundColor;
		}
	}

}