using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	[Obsolete]
	public class DraggableItemVisual : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler,
		IDragHandler
	{
		private ItemSlot _curSlot;

		#region IBeginDragHandler Implementation

		public void OnBeginDrag(PointerEventData eventData)
		{
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
			//look through all things under draggable
			RaycastHit[] hits;
			RaycastHit hit;
			ItemSlot slot = null;
			var rayPosition = Input.mousePosition;
			rayPosition.z -= 10;
			hits = Physics.RaycastAll(rayPosition, transform.forward, 100.0f);

			for (var i = 0; i < hits.Length; i++)
			{
				slot = null;
				hit = hits[i];
				slot = hit.transform.GetComponent<ItemSlot>();

				if (slot == null)
				{
					continue;
				}

				//If the draggable is over a slot, check to see if it's open and if so, assign it to it
				if (slot.ItemInSlot != null)
				{
					_curSlot.SetToSlot(this);
					return;
				}

				slot.SetToSlot(this);
			}
		}

		#endregion

		#region IPointerClickHandler Implementation

		public void OnPointerClick(PointerEventData eventData)
		{
		}

		#endregion

		//Sets the Item placement and size to be that of the slot
		public void SetItemPlacement(ItemSlot slot)
		{
			transform.SetParent(slot.ItemSpace);
			var rect = GetComponent<RectTransform>();
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0, 0);
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, 0);
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
			rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
		}

		/// <summary>
		///     Set the current slot
		/// </summary>
		/// <param name="slot">The slot to be set to</param>
		public void SetSlot(ItemSlot slot)
		{
			_curSlot = slot;
		}
	}

}