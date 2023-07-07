using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TezosSDK.Samples.DemoExample
{
	public class Draggable : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		[SerializeField] private ItemSnapPoint _currentSlot;
		[SerializeField] private Image image;

		public IItemModel Item { get; set; }
		public Action<Draggable> OnBeginDragging;
		public Action<Draggable> OnEndDragging;
		public Action<Draggable> OnClick;
		public ItemSnapPoint CurrentSlot => _currentSlot;

		public Sprite Sprite
		{
			get { return image.sprite; }
			set { image.sprite = value; }
		}

		private bool _isDragging = false;

		private void Start()
		{
			if (_currentSlot != null)
				ResetPosition();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!_isDragging)
				OnClick?.Invoke(this);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_isDragging = true;
			OnBeginDragging?.Invoke(this);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isDragging = false;
			OnEndDragging?.Invoke(this);
		}

		public void OnDrag(PointerEventData eventData)
		{
			transform.position = Input.mousePosition;
		}

		public void SetCurrentSlot(ItemSnapPoint slot)
		{
			ItemSnapPoint prevSlot = _currentSlot;
			_currentSlot = slot;
			_currentSlot.SetItemInSlot(this);
			transform.position = _currentSlot.transform.position;
			if (prevSlot != null)
				prevSlot.RemoveItemInSlot();
		}

		public void ResetPosition()
		{
			_currentSlot.SetItemInSlot(this);
			if (_currentSlot != null)
				transform.position = _currentSlot.transform.position;
		}
	}
}