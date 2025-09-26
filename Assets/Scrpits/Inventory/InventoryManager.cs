using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int,ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem;// the index of the array is the inventory list, and the value is the item code,����������ǿ���嵥����ֵ����Ʒ���롣

    public List<InventoryItem>[]inventoryLists;//��Ϊ��ά���飬��¼��ÿ��λ�ã�������ϡ����ӣ��Ŀ��list���

    [HideInInspector]public int [] inventoryListCapacityIntArray;//ÿ��λ�ÿ���item��������
    // the index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list
    // ����������ǿ���б�����InventoryLocationö�٣�����ֵ�Ǹÿ���б��������

    [SerializeField]private SO_ItemList ItemList=null;


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory Lists��������嵥
        CreateInventoryLists();

        //Create item details dictionary������Ŀ�����ֵ�
        CreateItemDetailsDictionary();


        // Initialize selected inventory item array��ʼ��ѡ�������Ŀ����
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }

    }

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)

        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        // Anitialise inventory list capacity array ��ʼ������嵥��������
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        //initialise player inventory list capacity��ʼ�������Ʒ������
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }

    //Populates the itemDetailsDictionary from the scriptable object items list�ӿɱ�̶�����Ŀ�б��������Ŀ����ʵ�
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int,ItemDetails>();

        foreach (ItemDetails itemDetails in ItemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    //Add an item to the inventory list for the inventorylocation and then destroy the gameObjectToDelete����λ�õĿ���б����һ����Ʒ��Ȼ������Ҫɾ������Ϸ����
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {

        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }

    //Add an item to the inventory list for the inventorylocation��һ����Ʒ��ӵ����λ�õĿ���嵥��
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode=item.ItemCode;
        List<InventoryItem> inventoryList=inventoryLists[(int)inventoryLocation];

        //Check if inventory already contains the item��������Ƿ��Ѱ�������Ʒ
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)

        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {

            AddItemAtPosition(inventoryList, itemCode);
        }
        // Send event that inventory has been updated���Ϳ���Ѹ��µ��¼�
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    //Add item to the end of the inventory����Ʒ��ӵ���Ʒ��ĩβ
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)

    {
        InventoryItem inventoryItem = new InventoryItem();
        
        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        //DebugPrintInventoryList(inventoryList);
    }

    //Add item to position in the inventory����Ʒ����������е�ָ��λ��
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
    //�ڿ��λ�� inventory �Ŀ���б��У��� fromItem ����������Ʒ�� toltem ����������Ʒ���н���
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        //if fromItem index and toItem Index are within the bounds of the list, not the same,and greater than or equal to zero
        //��� fromItem ������ toItem ���������б�Χ�ڣ��Ҳ���ͬ���Ҵ��ڵ�����
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];
            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
            //��Ʒ����

            // Send event that inventory has been updated ���Ϳ������¼� 
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
            //ͨ��EventHandler.CallInventoryUpdatedEvent�㲥���������UI��ʾ�ܹ����̸��¡�
        }
    }
    //���fromItem��toItem�Ƿ��ڿ���б���Ч��Χ�ڣ�< inventoryLists.Count��
    //ȷ��������������ͬ��fromItem != toItem��

    // Clear the selected inventory item for inventoryLocation�����ѡ������ڿ��λ�õĿ��
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;

    }


    //Find if an itemCode is already in the inventory. Returns the item position in the inventory list, or -1 if the item is not in the inventory
    //�����Ʒ�����Ƿ��Ѵ����ڿ���С�����Ʒ�����򷵻ظ���Ʒ�ڿ���б��е�λ�ã�����Ʒ�������򷵻�-1��
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
    //�ܽ᣺���ȣ������������� inventoryLists �� inventoryListCapacityIntArray��ǰ����Ƕ�ά���飬��¼��ÿ��λ��(����������ϵģ����������ĵ� )�Ŀ��list�����
    //����ļ�¼��ÿ��λ�ÿ���item�������� �� 
    //Ȼ����Awake��ͨ�� CreateInventoryLists ��ʼ�� inventoryLists �� inventoryListCapacityIntArray ��ֵ��
    //���Ŵ������item�ķ���Addltem(InventoryLocation inventoryLocation, ltem item)�������ĸ�λ�÷�ʲôItem����Ҫ������һ���λ�õĿ�棬������Ƿ��Ѿ����ڣ������򷵻ؿ���嵥������ֵ��
    //Ȼ�����Ǹ�������Ӧ��Inventoryltem�����¿����������Ҳ������item����ôֱ����λ�ö�Ӧ�Ŀ���嵥δβ�������Inventoryltem��

    //Returns the itemDetails(from the SO_Itemlist) for the itemCode, or null if the item code doesn��t exist����ָ����Ʒ�����Ӧ����Ʒ���飨����SO_Itemlist��������Ʒ���벻�����򷵻�null��
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



    // Returns the itemDetails��from the SO_ItemList��for the currently selected item in the inventoryLocation,or null if an item isn't selected
    // ���ص�ǰ�ڿ��λ����ѡ����Ʒ����Ʒ���飨����SO_ItemList������δѡ����Ʒ�򷵻�null��
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
        //��ȡ���λ�õ�ѡ����Ʒ - ������Ʒ���룬��δѡ���򷵻�-1
        private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
        {
        return selectedInventoryItem[(int)inventoryLocation];
        }


    //Get the item type description for an item type - returns the item type description as a string for a given ItemType
    //��ȡ�����͵����� - ����ָ�������͵����������ַ�����ʽ��
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

    //Remove an item from the inventory, and create a game object at the position it was dropped�ӿ�����Ƴ�һ����Ʒ�������䱻������λ�ô���һ����Ϸ����
    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];//��inventoryLists�ļ����и���inventoryLocationָ����λ�û�ȡ��Ӧ����Ʒ�б�

        // Check if inventory already contains the item�����Ʒ�Ƿ��Ѵ����ڱ�����
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if(itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList,itemCode, itemPosition);
        }
        //FindItemInInventory ���������ڱ����в���ָ����Ʒ��λ�ã��������򷵻����������򷵻�-1��
        //���ҵ���Ʒ��itemPosition != -1��������� RemoveItemAtPosition ִ���Ƴ���

        //Send event that inventory has been updated���Ϳ���Ѹ��µ��¼�
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        //�Ƴ�������ɺ�ͨ�� EventHandler.CallInventoryUpdatedEvent �������������¼���֪ͨ����ϵͳͬ�����ݡ�
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
        //����Ʒʣ����������1��������������inventoryItem.itemQuantity = quantity��

        else
        {
            inventoryList.RemoveAt(position);
        }
        //����������0�����£�����б��г���ɾ������Ʒ��inventoryList.RemoveAt(position)����
    }



    // Set the selected inventory item for inventoryLocation to itemCode��ѡ���Ŀ����Ŀ����Ϊ���λ�õ���Ŀ����
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
    //����ע�� ctrl+K+C ��� ctrl+K+U
}


