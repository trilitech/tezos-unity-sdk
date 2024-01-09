using System;
using System.Collections.Generic;
using TezosSDK.MarketplaceSample.MarketplaceExample.Core;
using TezosSDK.MarketplaceSample.MarketplaceExample.UI;
using UnityEngine;
using UnityEngine.Events;
using Logger = TezosSDK.Helpers.Logger;

namespace TezosSDK.MarketplaceSample.MarketplaceExample
{

	[RequireComponent(typeof(SnapController))]
	public class InventoryManager : MonoBehaviour
	{
		public UnityEvent onItemMint;
		public UnityEvent onInventoryRefresh;

		[SerializeField] private Draggable itemPrefab;
		[SerializeField] private List<ItemSnapPoint> inventorySlots;
		[SerializeField] private List<ItemSnapPoint> equippedSlots;
		[SerializeField] private List<Draggable> draggables = new();
		[SerializeField] private StatsView statsView;
		[SerializeField] private ItemView itemView;
		private const string PlayerPrefsEquipKey = "_eq_";

		private const string PlayerPrefsInvKey = "_inv_";

		private readonly Dictionary<int, Draggable> _itemIDtoDraggable = new();
		private readonly List<int> _lastHeldItems = new();
		private SnapController _snapController;

		private void Start()
		{
			_snapController = GetComponent<SnapController>();
		}

		private void OnApplicationQuit()
		{
			// no need for local storage, since data need to be coming from the blockchain
			//SaveInventoryLocally();
		}

		public void Init(List<IItemModel> items)
		{
			//ClearInventory();
			UpdateItems(items);
			UpdateSnapController();

			// no need for local storage, since data need to be coming from the blockchain
			//LoadLocalInventory();
		}

		public void OnItemClicked(Draggable item)
		{
			itemView.transform.parent.gameObject.SetActive(true);
			itemView.gameObject.SetActive(true);
			itemView.DisplayItem(item.Item, item.Sprite);
			itemView.GetComponent<ItemController>().SetItem(item.Item);
		}

		public void OnMintButtonClicked()
		{
			onItemMint.Invoke();
		}

		public void OnRefreshButtonClicked()
		{
			onInventoryRefresh.Invoke();
		}

        /// <summary>
        ///     Adds any missing items and removes any items that should be gone.
        /// </summary>
        public void UpdateItems(List<IItemModel> items)
		{
			ClearInventory();
			UpdateStatsView();
			_lastHeldItems.Clear();

			foreach (var item in items)
			{
				_lastHeldItems.Add(item.ID);

				if (!_itemIDtoDraggable.ContainsKey(item.ID))
				{
					AddItem(item);
				}
			}

			foreach (var itemToRemove in _itemIDtoDraggable)
			{
				if (!_lastHeldItems.Contains(itemToRemove.Value.Item.ID))
				{
					itemToRemove.Value.CurrentSlot.RemoveItemInSlot();
					//Uncertain if RemoveItemInSlot's event also removes the item from the dictionary or not.
					//_itemIDtoDraggable.Remove(itemToRemove.Key);
				}
			}
		}

		public void UpdateStatsView()
		{
			var heroStats = new StatParams();

			foreach (var slot in equippedSlots)
			{
				if (!slot.HasItem)
				{
					continue;
				}

				var sp = slot.CurrentItemInSlot.Item.Stats;

				foreach (var prop in sp.GetType().GetProperties())
				{
					var value = (float)prop.GetValue(sp) + (float)prop.GetValue(heroStats);
					prop.SetValue(heroStats, value);
				}
			}

			statsView.DisplayStats(heroStats);
		}

		private void AddItem(IItemModel itemModel)
		{
			var newItem = Instantiate(itemPrefab, transform);
			var itemRes = Resources.Load<ItemReseource>(itemModel.ResourcePath);

			if (itemRes != null)
			{
				newItem.Sprite = itemRes.ItemSprite;
			}
			else
			{
				Logger.LogError("Could find resource file for " + itemModel.ResourcePath);
			}

			newItem.Item = itemModel;
			newItem.OnClick = OnItemClicked;
			draggables.Add(newItem);
			var emptySlot = GetFirstEmptySlot();

			if (emptySlot != null)
			{
				newItem.SetCurrentSlot(emptySlot);
				_itemIDtoDraggable[itemModel.ID] = newItem;
			}
			else
			{
				Logger.LogError("Trying to add an item but Inventory is full!");
			}
		}

		private void ClearInventory()
		{
			foreach (var snapSlot in inventorySlots)
			{
				snapSlot.RemoveItemInSlot();
			}

			foreach (var snapSlot in equippedSlots)
			{
				snapSlot.RemoveItemInSlot();
			}

			foreach (var draggable in draggables)
			{
				Destroy(draggable.gameObject);
			}

			draggables.Clear();
			_itemIDtoDraggable.Clear();
		}

		private ItemSnapPoint GetFirstEmptySlot()
		{
			foreach (var snapPoint in inventorySlots)
			{
				if (!snapPoint.HasItem)
				{
					return snapPoint;
				}
			}

			return null;
		}

		private void LoadLocalInventory()
		{
			var db = (ExampleManager)ExampleFactory.Instance.GetExampleManager();
			var playerID = db.CurrentUser.Identifier;
			var invSaveLoc = playerID + PlayerPrefsInvKey;
			var eqSaveLoc = playerID + PlayerPrefsEquipKey;

			for (var i = 0; i < inventorySlots.Count; i++)
			{
				if (PlayerPrefs.HasKey(invSaveLoc + i))
				{
					var itemID = PlayerPrefs.GetInt(invSaveLoc + i);
					_itemIDtoDraggable[itemID].SetCurrentSlot(inventorySlots[i]);
				}
			}

			for (var i = 0; i < equippedSlots.Count; i++)
			{
				if (PlayerPrefs.HasKey(eqSaveLoc + i))
				{
					var itemID = PlayerPrefs.GetInt(eqSaveLoc + i);
					_itemIDtoDraggable[itemID].SetCurrentSlot(equippedSlots[i]);
				}
			}

			Logger.LogDebug("Inventory loaded.");
		}

		private void SaveInventoryLocally()
		{
			PlayerPrefs.DeleteAll();
			var playerID = ((ExampleManager)ExampleFactory.Instance.GetExampleManager()).CurrentUser.Identifier;
			var invSaveLoc = playerID + PlayerPrefsInvKey;
			var eqSaveLoc = playerID + PlayerPrefsEquipKey;

			for (var i = 0; i < inventorySlots.Count; i++)
			{
				if (inventorySlots[i].HasItem)
				{
					PlayerPrefs.SetInt(invSaveLoc + i, inventorySlots[i].CurrentItemInSlot.Item.ID);
				}
			}

			for (var i = 0; i < equippedSlots.Count; i++)
			{
				if (equippedSlots[i].HasItem)
				{
					PlayerPrefs.SetInt(eqSaveLoc + i, equippedSlots[i].CurrentItemInSlot.Item.ID);
				}
			}

			Logger.LogDebug("Inventory saved locally.");
		}

		private void UpdateSnapController()
		{
			try
			{
				_snapController.Draggables = draggables;
				_snapController.SnapPoints = inventorySlots;

				foreach (var slot in equippedSlots)
				{
					_snapController.SnapPoints.Add(slot);
				}
			}
			catch (NullReferenceException ex)
			{
				Debug.Log($"Error during UpdateSnapController: {ex.Message}");
			}
		}
	}

}