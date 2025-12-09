using System.Collections.Generic;
using UnityEngine;


public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISaveable
{
    private UIInventoryBar inventoryBar;

    private Dictionary<int,ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem;// the index of the array is the inventory list, and the value is the item code,数组的索引是库存清单，其值是物品代码。

    public List<InventoryItem>[]inventoryLists;//此为二维数组，记录了每个位置（玩家身上、箱子）的库存list情况

    [HideInInspector]public int [] inventoryListCapacityIntArray;//每个位置库存的item上限数量
    // the index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list
    // 数组的索引是库存列表（来自InventoryLocation枚举），其值是该库存列表的容量。

    [SerializeField]private SO_ItemList ItemList=null;

    private string _iSaveableUniqueID;

    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory Lists创建库存清单
        CreateInventoryLists();

        //Create item details dictionary创建项目详情字典
        CreateItemDetailsDictionary();


        // Initialize selected inventory item array初始化选定库存项目数组
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }
        // Get unique ID for gameobject and create save data object 获取游戏对象的唯一ID并创建保存数据对象
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void Start()
    {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
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

    //Populates the itemDetailsDictionary from the scriptable object items list从可编程对象项目列表中填充项目详情词典
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
    public void AddItem(InventoryLocation inventoryLocation, Item item)//拾取场景中的物品并添加到物品栏中
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


    //Add an item of type itemCode to the inventory list for the inventoryLocation将类型为itemCode的物品添加到库存位置的库存列表中
    public void AddItem(InventoryLocation inventoryLocation, int itemCode)//将收获的作物直接添加到物品栏中
    {
        // 根据传入的库存位置枚举，从库存字典中获取对应的库存列表
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains the item在指定库存中查找该物品是否已存在，返回其索引位置，若不存在则返回-1
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        // 如果物品已存在于库存中（找到了有效索引位置）
        if (itemPosition != -1)
        {
            // 调用方法在已知位置增加物品数量（堆叠物品）
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            // 调用方法在库存中新增加物品（新增物品槽）
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

        //DebugPrintInventoryList(inventoryList);
    }

    //Add item to position in the inventory将物品添加至背包中的指定位置
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {

        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryList[position] = inventoryItem;

        //DebugPrintInventoryList(inventoryList);
    }

    //Swap item at fromItem index with item at toltem index in inventoryLocation inventory list
    //在库存位置 inventory 的库存列表中，将 fromItem 索引处的物品与 toltem 索引处的物品进行交换
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        //if fromItem index and toItem Index are within the bounds of the list, not the same,and greater than or equal to zero
        //如果 fromItem 索引和 toItem 索引都在列表范围内，且不相同，且大于等于零
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];
            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
            //物品交换

            // Send event that inventory has been updated 发送库存更新事件 
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
            //通过EventHandler.CallInventoryUpdatedEvent广播库存变更，让UI显示能够立刻更新。
        }
    }
    //检查fromItem和toItem是否在库存列表有效范围内（< inventoryLists.Count）
    //确保两个索引不相同（fromItem != toItem）

    // Clear the selected inventory item for inventoryLocation清除所选库存项在库存位置的库存
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;

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



    // Returns the itemDetails（from the SO_ItemList）for the currently selected item in the inventoryLocation,or null if an item isn't selected
    // 返回当前在库存位置中选定物品的物品详情（来自SO_ItemList），若未选定物品则返回null。
    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }

    }

        //Get the selected item for inventoryLocation - returns itemCode or -1 if nothing is selected
        //获取库存位置的选定商品 - 返回商品代码，若未选中则返回-1
        private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
        {
        return selectedInventoryItem[(int)inventoryLocation];
        }


    //Get the item type description for an item type - returns the item type description as a string for a given ItemType
    //获取项类型的描述 - 返回指定项类型的描述（以字符串形式）
    public string GetItemTypeDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_tool:
            itemTypeDescription = Settings.BreakingTool;
            break;

            case ItemType.Chopping_tool:
            itemTypeDescription = Settings.ChoppingTool;
            break;

            case ItemType.Hoeing_tool:
            itemTypeDescription = Settings.HoeingTool;
            break;

            case ItemType.Reaping_tool:
            itemTypeDescription = Settings.ReapingTool;
            break;

            case ItemType.Watering_tool:
            itemTypeDescription = Settings.WateringTool;
            break;

            case ItemType.Collecting_tool:
            itemTypeDescription = Settings.CollectingTool;
            break;

            default:
            itemTypeDescription = itemType.ToString();
            break;
        }
        return itemTypeDescription;
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }


    public GameObjectSave ISaveableSave()
    {
        // 创建新的场景保存对象
        SceneSave sceneSave = new SceneSave();

        // 清理旧的持久化场景数据
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // 保存库存物品列表数组
        sceneSave.listInvItemArray = inventoryLists;

        // 保存库存容量数组到字典中
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        // 将场景数据添加到游戏对象保存中
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }



    public void ISaveableLoad(GameSave gameSave)
    {
        // 根据唯一ID查找对应的存档数据
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // 从持久化场景中获取库存数据
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // 恢复库存物品列表
                if (sceneSave.listInvItemArray != null)
                {
                    inventoryLists = sceneSave.listInvItemArray;

                    // 触发库存更新事件，通知其他系统
                    for (int i = 0; i < (int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }

                    // 清理玩家携带的物品状态
                    Player.Instance.ClearCarriedItem();
                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                // 恢复库存容量数据
                if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[] inventoryCapacityArray))
                {
                    inventoryListCapacityIntArray = inventoryCapacityArray;
                }
            }
        }
    }


    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since the inventory manager is on a persistent scene;
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since the inventory manager is on a persistent scene;
    }

    //Remove an item from the inventory, and create a game object at the position it was dropped从库存中移除一个物品，并在其被丢弃的位置创建一个游戏对象
    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];//在inventoryLists的集合中根据inventoryLocation指定的位置获取对应的物品列表

        // Check if inventory already contains the item检查物品是否已存在于背包中
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        //FindItemInInventory 方法用于在背包中查找指定物品的位置，若存在则返回索引，否则返回-1。
        //若找到物品（itemPosition != -1），则调用 RemoveItemAtPosition 执行移除。
        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList,itemCode, itemPosition);
        }
      

        //Send event that inventory has been updated发送库存已更新的事件
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        //移除操作完成后，通过 EventHandler.CallInventoryUpdatedEvent 触发背包更新事件，通知其他系统同步数据。
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }
        //若物品剩余数量大于1，仅更新数量（inventoryItem.itemQuantity = quantity）

        else
        {
            inventoryList.RemoveAt(position);
        }
        //若数量减至0或以下，则从列表中彻底删除该物品（inventoryList.RemoveAt(position)）。
    }



    // Set the selected inventory item for inventoryLocation to itemCode将选定的库存项目设置为库存位置的项目代码
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }





    //private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    //{
    //    foreach (InventoryItem inventoryItem in inventoryList)
    //    { 
    //        Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + " Item Quantity: " + inventoryItem.itemQuantity); 
    //    }
    //    Debug.Log("*******************************************************"); 
    //}
    //批量注释 ctrl+K+C 解除 ctrl+K+U
}


