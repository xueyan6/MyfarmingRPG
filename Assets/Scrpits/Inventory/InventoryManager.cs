using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int,ItemDetails> itemDetailsDictionary;

    public List<InventoryItem>[]inventoryLists;//此为二维数组，记录了每个位置（玩家身上、箱子）的库存list情况

    [HideInInspector]public int [] inventoryListCapacityIntArray;//每个位置库存的item上限数量
    // the index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list数组的索引是库存列表（来自InventoryLocation枚举），其值是该库存列表的容量。

    [SerializeField]private SO_ItemList ItemList=null;


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory Lists创建库存清单

        CreateInventoryLists();

        //Create item details dictionary创建项目详情字典
        CreateItemDetailsDictionary();
    }

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)

        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        // Anitialise inventory list capacity array 初始化库存清单容量数组
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //initialise player inventory list capacity初始化玩家物品栏容量
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    //Populates the itemPetailsDictionary from the scriptable object items list从可编程对象项目列表中填充itemPetailsDictionary
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int,ItemDetails>();

        foreach (ItemDetails itemDetails in ItemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    //Add an item to the inventory list for the inventorylocation and then destroy the gameObjectToDelete向库存位置的库存列表添加一项物品，然后销毁要删除的游戏对象。
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {

        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }

    //Add an item to the inventory list for the inventorylocation将一项物品添加到库存位置的库存清单中
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode=item.ItemCode;
        List<InventoryItem> inventoryList=inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item检查库存中是否已包含该物品
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)

        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {

            AddItemAtPosition(inventoryList, itemCode);
        }
        // Send event that inventory has been updated发送库存已更新的事件
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    //Add item to the end of the inventory将物品添加到物品栏末尾
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)

    {
        InventoryItem inventoryItem = new InventoryItem();
        
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        DebugPrintInventoryList(inventoryList);
    }

    //Add item to position in the inventory将物品添加至背包中的指定位置
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {

        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryList[position] = inventoryItem;

        DebugPrintInventoryList(inventoryList);
    }


    //Find if an itemCode is already in the inventory. Returns the item position in the inventory list, or -1 if the item is not in the inventory
    //检查物品代码是否已存在于库存中。若物品存在则返回该物品在库存列表中的位置，若物品不存在则返回-1。
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {

        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }
    //总结：首先，创建两个变量 inventoryLists 和 inventoryListCapacityIntArray。前面的是二维数组，记录了每个位置(比如玩家身上的，玩家箱子里的等 )的库存list情况。
    //后面的记录了每个位置库存的item上限数量 。 
    //然后，在Awake中通过 CreateInventoryLists 初始化 inventoryLists 和 inventoryListCapacityIntArray 的值。
    //接着创建添加item的方法Addltem(InventoryLocation inventoryLocation, ltem item)，即在哪个位置放什么Item。需要先搜索一遍该位置的库存，检查下是否已经存在，存在则返回库存清单的索引值。
    //然后在那个索引对应的Inventoryltem更新下库存数。如果找不到这个item，那么直接在位置对应的库存清单未尾增加这个Inventoryltem。

    //Returns the itemDetails(from the SO_Itemlist) for the itemCode, or null if the item code doesn’t exist返回指定商品代码对应的商品详情（来自SO_Itemlist），若商品代码不存在则返回null。
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

    private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    {
        foreach (InventoryItem inventoryItem in inventoryList)
        { 
            Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + " Item Quantity: " + inventoryItem.itemQuantity); 
        }
        Debug.Log("*******************************************************"); 
    }
}


