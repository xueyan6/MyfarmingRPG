
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
  private Dictionary<int,ItemDetails> itemDetailsDictionary;
    [SerializeField]private SO_ItemList ItemList=null;


    private void Start()
    {
        //Create item details dictionary
        CreateItemDetailsDictionary();
    }


    //Populates the itemPetailsDictionary from the scriptable object items list
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int,ItemDetails>();

        foreach (ItemDetails itemDetails in ItemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    //Returns the itemDetails(from the SO_Itemlist) for the itemCode, or null if the item code doesn¡¯t exist
    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if(itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }
}


