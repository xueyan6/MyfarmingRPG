using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<int,ItemDetails> itemDetailsDictionary;

    public List<InventoryItem>[]inventoryLists;//��Ϊ��ά���飬��¼��ÿ��λ�ã�������ϡ����ӣ��Ŀ��list���

    [HideInInspector]public int [] inventoryListCapacityIntArray;//ÿ��λ�ÿ���item��������
    // the index of the array is the inventory list (from the InventoryLocation enum), and the value is the capacity of that inventory list����������ǿ���б�����InventoryLocationö�٣�����ֵ�Ǹÿ���б��������

    [SerializeField]private SO_ItemList ItemList=null;


    protected override void Awake()
    {
        base.Awake();

        //Create Inventory Lists��������嵥

        CreateInventoryLists();

        //Create item details dictionary������Ŀ�����ֵ�
        CreateItemDetailsDictionary();
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

    //Populates the itemPetailsDictionary from the scriptable object items list�ӿɱ�̶�����Ŀ�б������itemPetailsDictionary
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

        DebugPrintInventoryList(inventoryList);
    }

    //Add item to position in the inventory����Ʒ����������е�ָ��λ��
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

    private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    {
        foreach (InventoryItem inventoryItem in inventoryList)
        { 
            Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + " Item Quantity: " + inventoryItem.itemQuantity); 
        }
        Debug.Log("*******************************************************"); 
    }
}


