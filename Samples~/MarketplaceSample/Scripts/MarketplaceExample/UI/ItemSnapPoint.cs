using System.Linq;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	[RequireComponent(typeof(Image))]
	public class ItemSnapPoint : MonoBehaviour
	{
		[SerializeField] private bool snappable = true;
		[SerializeField] private bool allowAllItemTypes = true;
		[SerializeField] private ItemType[] allowedTypes;

		[Header("Events:")] [SerializeField] public UnityEvent OnItemSet;
		[SerializeField] public UnityEvent OnItemRemoved;

		private bool _highledEnabled = true;
		private Color _highlightColor;
		private Image _image;
		private Color _originalColor;

		public bool AllowsAllItemTypes
		{
			get => allowAllItemTypes;
		}

		public Draggable CurrentItemInSlot { get; private set; }

		public bool HasItem
		{
			get => CurrentItemInSlot != null;
		}

		public bool IsSnappable
		{
			get => snappable;
		}

		private void Start()
		{
			_image = GetComponent<Image>();
			_originalColor = _image.color;
			_highlightColor = Color.white;
		}

		public bool AcceptsItem(IItemModel item)
		{
			return allowedTypes.Contains(item.Type);
		}

		public void EnableHighlighting(bool enable)
		{
			_highledEnabled = enable;

			if (!_highledEnabled)
			{
				_image.color = _originalColor;
			}
		}

		public void Highlight(bool highlight)
		{
			_image.color = highlight ? _highlightColor : _originalColor;
		}

		public void RemoveItemInSlot()
		{
			CurrentItemInSlot = null;
			OnItemRemoved?.Invoke();
		}

		public void SetItemInSlot(Draggable item)
		{
			CurrentItemInSlot = item;
			OnItemSet?.Invoke();
		}
	}

}