using System;
using UnityEngine;
using UnityEngine.UI;

namespace TezosSDK.MarketplaceSample.MarketplaceExample.UI
{

	[Obsolete]
	[RequireComponent(typeof(Image))]
	public class ItemSlot : MonoBehaviour
	{
		[SerializeField] private Transform _itemSpace;

		public DraggableItemVisual ItemInSlot { get; private set; }

		public Transform ItemSpace
		{
			get => _itemSpace;
		}

		/// <summary>
		///     assigns an item to slot and lets the item know what slot it's set to
		/// </summary>
		/// <param name="item">The item to be asigned to this slot</param>
		public void SetToSlot(DraggableItemVisual item)
		{
			if (ItemInSlot != item)
			{
				ItemInSlot = item;
				ItemInSlot.SetSlot(this);
			}

			item.SetItemPlacement(this);
		}
	}

}