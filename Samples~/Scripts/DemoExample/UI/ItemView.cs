using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemView : MonoBehaviour, IPointerClickHandler
{
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image itemImage;
	[SerializeField] private TMPro.TextMeshProUGUI priceText;
	[SerializeField] private TMPro.TextMeshProUGUI nameText;
	[SerializeField] private TMPro.TextMeshProUGUI typeText;
	[SerializeField] private StatsView statsView;
	[SerializeField] private Color selectColor = Color.white;
	[SerializeField] private Sprite defaultImage;

	private IItemModel _item;
	private Color _originalBackgroundColor;
	private Sprite _cachedSprite;
	private Sprite _initSprite;

	public IItemModel Item => _item;
	public Action<ItemView> OnItemSelected;
	public Sprite CachedSprite => _cachedSprite;

	private void Start()
	{
		_originalBackgroundColor = backgroundImage.color;
		_initSprite = itemImage.sprite;
	}

	public void DisplayItem(IItemModel item, Sprite cahcedSprite = null)
	{
		if (item == null)
		{
			Debug.LogWarning("item in market currently null -test message-");

			priceText.text = "Price";
			nameText.text = "Name";
			typeText.text = "[Type]";
			StatParams statParams = new StatParams(0, 0, 0, 0, 0);
			statsView.DisplayStats(statParams);
			itemImage.sprite = _initSprite;
			return;
		}
		else
		{

			if (priceText != null)
				priceText.text = item.Price.ToString(CultureInfo.InvariantCulture);
			if (nameText != null)
				nameText.text = item.Name;
			if (typeText != null)
				typeText.text = item.Type.ToString();
			if (statsView != null)
				statsView.DisplayStats(item.Stats);
			if (cahcedSprite != null)
			{
				itemImage.sprite = cahcedSprite;
				_cachedSprite = itemImage.sprite;
			}
			else if (itemImage != null)
			{
				ItemReseource itemRes = Resources.Load<ItemReseource>(item.ResourcePath);
				itemImage.sprite = itemRes.ItemSprite;
				_cachedSprite = itemImage.sprite;
			}
		}
		_item = item;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnItemSelected?.Invoke(this);
	}

	public void Select()
	{
		backgroundImage.color = selectColor;
	}

	public void Unselect()
	{
		backgroundImage.color = _originalBackgroundColor;
	}

	public void ClearItem()
	{
		if (priceText != null)
			priceText.text = string.Empty;
		if (nameText != null)
			nameText.text = string.Empty;
		if (typeText != null)
			typeText.text = string.Empty;
		if (itemImage != null)
			itemImage.sprite = defaultImage;
		if (statsView != null)
			statsView.Clear();
		_item = null;
	}
}
