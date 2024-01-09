using System;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	public class Draggable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		[SerializeField] private ItemSnapPoint _currentSlot;
		[SerializeField] private Image image;
		public Action<Draggable> OnBeginDragging;
		public Action<Draggable> OnClick;
		public Action<Draggable> OnEndDragging;

		private bool _isDragging;

		public ItemSnapPoint CurrentSlot
		{
			get => _currentSlot;
		}

		public IItemModel Item { get; set; }

		public Sprite Sprite
		{
			get => image.sprite;
			set => image.sprite = value;
		}

		#region IBeginDragHandler Implementation

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isDragging = true;
			OnBeginDragging?.Invoke(this);
		}

		#endregion

		#region IDragHandler Implementation

		public void OnDrag(PointerEventData eventData)
		{
			transform.position = Input.mousePosition;
		}

		#endregion

		#region IEndDragHandler Implementation

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
			OnEndDragging?.Invoke(this);
		}

		#endregion

		#region IPointerClickHandler Implementation

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_isDragging)
			{
				OnClick?.Invoke(this);
			}
		}

		#endregion

		private void Start()
		{
			if (_currentSlot != null)
			{
				ResetPosition();
			}
		}

		public void ResetPosition()
		{
			_currentSlot.SetItemInSlot(this);

			if (_currentSlot != null)
			{
				transform.position = _currentSlot.transform.position;
			}
		}

		public void SetCurrentSlot(ItemSnapPoint slot)
		{
			var prevSlot = _currentSlot;
			_currentSlot = slot;
			_currentSlot.SetItemInSlot(this);
			transform.position = _currentSlot.transform.position;

			if (prevSlot != null)
			{
				prevSlot.RemoveItemInSlot();
			}
		}
	}

}