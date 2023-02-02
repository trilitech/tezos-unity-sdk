using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(SnapController))]
public class InventoryManager : MonoBehaviour
{
	public UnityEvent onItemMint;
	public UnityEvent onInventoryRefresh;
	
	[SerializeField] private Draggable itemPrefab;
	[SerializeField] private List<ItemSnapPoint> inventorySlots;
	[SerializeField] private List<ItemSnapPoint> equippedSlots;
	[SerializeField] private List<Draggable> draggables = new List<Draggable>();
	[SerializeField] private StatsView statsView;
	[SerializeField] private ItemView itemView;

	private Dictionary<int, Draggable> _itemIDtoDraggable = new Dictionary<int, Draggable>();
	private SnapController _snapController;
	private List<int> _lastHeldItems = new List<int>();

	private const string PlayerPrefsInvKey = "_inv_";
	private const string PlayerPrefsEquipKey = "_eq_";

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

	private void AddItem(IItemModel itemModel)
	{
		Draggable newItem = Instantiate(itemPrefab, this.transform);
		ItemReseource itemRes = Resources.Load<ItemReseource>(itemModel.ResourcePath);
		if (itemRes != null)
			newItem.Sprite = itemRes.ItemSprite;
		else
			Debug.LogError("Could find resource file for " + itemModel.ResourcePath);
		newItem.Item = itemModel;
		newItem.OnClick = OnItemClicked;
		draggables.Add(newItem);
		ItemSnapPoint emptySlot = GetFirstEmptySlot();
		if (emptySlot != null)
		{
			newItem.SetCurrentSlot(emptySlot);
			_itemIDtoDraggable[itemModel.ID] = newItem;
		}
		else
		{
			Debug.LogError("Trying to add an item but Inventory is full!");
			return;
		}
	}

	/// <summary>
	/// Adds any missing items and removes any items that should be gone.
	/// </summary>
	public void UpdateItems(List<IItemModel> items)
	{
		ClearInventory();
		UpdateStatsView();
		_lastHeldItems.Clear();
		foreach (IItemModel item in items)
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

	private void UpdateSnapController()
	{
		_snapController.Draggables = draggables;
		_snapController.SnapPoints = inventorySlots;
		foreach(var slot in equippedSlots)
			_snapController.SnapPoints.Add(slot);
	}

	private ItemSnapPoint GetFirstEmptySlot()
	{
		foreach (var snapPoint in inventorySlots)
			if (!snapPoint.HasItem)
				return snapPoint;
		return null;
	}
	
	private void SaveInventoryLocally()
	{
		PlayerPrefs.DeleteAll();
		string playerID = ((ExampleManager)ExampleFactory.Instance.GetExampleManager()).CurrentUser.Identifier;
		string invSaveLoc = playerID + PlayerPrefsInvKey;
		string eqSaveLoc = playerID + PlayerPrefsEquipKey;
		for (int i = 0; i < inventorySlots.Count; i++)
		{
			if (inventorySlots[i].HasItem)
				PlayerPrefs.SetInt(invSaveLoc + i, inventorySlots[i].CurrentItemInSlot.Item.ID);
		}
		for (int i = 0; i < equippedSlots.Count; i++)
		{
			if (equippedSlots[i].HasItem)
				PlayerPrefs.SetInt(eqSaveLoc + i, equippedSlots[i].CurrentItemInSlot.Item.ID);
		}
		Debug.Log("Inventory saved locally.");
	}

	private void LoadLocalInventory()
	{
		ExampleManager db = (ExampleManager)ExampleFactory.Instance.GetExampleManager();
		string playerID = db.CurrentUser.Identifier;
		string invSaveLoc = playerID + PlayerPrefsInvKey;
		string eqSaveLoc = playerID + PlayerPrefsEquipKey;
		for (int i = 0; i < inventorySlots.Count; i++)
		{
			if (PlayerPrefs.HasKey(invSaveLoc + i))
			{
				int itemID = PlayerPrefs.GetInt(invSaveLoc + i);
				_itemIDtoDraggable[itemID].SetCurrentSlot(inventorySlots[i]);
			}
		}
		for (int i = 0; i < equippedSlots.Count; i++)
		{
			if (PlayerPrefs.HasKey(eqSaveLoc + i))
			{
				int itemID = PlayerPrefs.GetInt(eqSaveLoc + i);
				_itemIDtoDraggable[itemID].SetCurrentSlot(equippedSlots[i]);
			}
		}
		Debug.Log("Inventory loaded.");
	}

	public void UpdateStatsView()
	{
		StatParams heroStats = new StatParams();
		
		foreach (var slot in equippedSlots)
		{
			if (!slot.HasItem)
				continue;
			StatParams sp = slot.CurrentItemInSlot.Item.Stats;
			foreach (var prop in sp.GetType().GetProperties())
			{
				float value = (float)prop.GetValue(sp) + (float)prop.GetValue(heroStats);
				prop.SetValue(heroStats, value);
			}
		}
		statsView.DisplayStats(heroStats);
	}

	public void OnMintButtonClicked()
	{
		onItemMint.Invoke();
	}

	public void OnRefreshButtonClicked()
	{
		onInventoryRefresh.Invoke();
	}

	public void OnItemClicked(Draggable item)
	{
		itemView.transform.parent.gameObject.SetActive(true);
		itemView.gameObject.SetActive(true);
		itemView.DisplayItem(item.Item, item.Sprite);
		itemView.GetComponent<ItemController>().SetItem(item.Item);
	}
}
