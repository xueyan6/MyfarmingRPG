using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int,ItemDetails> itemDetailsDictionary;

    public List<InventoryItem>[]inventoryLists;

    [HideInInspector]public int [] inventoryListCapacityIntArray;// the index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list

    [SerializeField]private SO_ItemList ItemList=null;


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory Lists

        CreateInventoryLists();

        //Create item details dictionary
        CreateItemDetailsDictionary();
    }

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];
        for (int i = 0; i < (int)InventoryLocation.count; i++)

        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        // Anitialise inventory list capacity array 
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
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


