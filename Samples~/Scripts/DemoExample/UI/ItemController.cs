using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    private IItemModel item;

    [SerializeField] private TMPro.TMP_InputField transferAddressInput;
    [SerializeField] private TMPro.TMP_InputField amountInput;
    [SerializeField] private TMPro.TMP_InputField priceInput;
    [SerializeField] private Button transferButton;
    [SerializeField] private Button addToMarketButton;
    [SerializeField] private Button removeFromMarketButton;

    private bool _hasMoreThanOneItem = false;
    
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
        string destAddress = transferAddressInput.text;
        string amountString = amountInput.text;
        int amount = 1;
        if (_hasMoreThanOneItem)
            amount = (amountString == String.Empty)? 1 : int.Parse(amountString);
        ExampleFactory.Instance.GetExampleManager().TransferItem(item.ID, amount, destAddress);
    }

    public void AddItemToMarket()
    {
        int price = int.Parse(priceInput.text);
        ExampleFactory.Instance.GetExampleManager().AddItemToMarket(item.ID, price);
    }

    public void RemoveItemFromMarket()
    {
        ExampleFactory.Instance.GetExampleManager().RemoveItemFromMarket(item.ID);
    }
    
    public void OnIsItemOnMarket(bool success)
    { 
       addToMarketButton.gameObject.SetActive(!success);
       removeFromMarketButton.gameObject.SetActive(success);
       transferButton.gameObject.SetActive(!success);
       priceInput.interactable = !success;
    }
}
