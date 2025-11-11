using System.Collections.Generic;
using UnityEngine;

public class PauseMenuInventoryManagement : MonoBehaviour
{
    [SerializeField] private PauseMenuInventoryManagementSlot[] inventoryManagementSlot = null;  // 序列化的库存管理槽位数组

    public GameObject inventoryManagementDraggedItemPrefab;  // 拖拽时显示的物品预制体

    [SerializeField] private Sprite transparent16x16 = null;  // 透明精灵，用于清空槽位显示

    [HideInInspector] public GameObject inventoryTextBoxGameobject;  // 鼠标悬停时显示物品信息的文本框对象

    private void OnEnable()  // 当组件启用时调用的方法
    {
        EventHandler.InventoryUpdatedEvent += PopulatePlayerInventory;  // 订阅库存更新事件

        // 填充玩家库存
        if (InventoryManager.Instance != null)  // 检查库存管理器实例是否存在
        {
            PopulatePlayerInventory(InventoryLocation.player, InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player]);  // 初始化填充玩家库存数据
        }
    }

    private void OnDisable()  // 当组件禁用时调用的方法
    {
        EventHandler.InventoryUpdatedEvent -= PopulatePlayerInventory;  // 取消订阅库存更新事件

        DestroyInventoryTextBoxGameobject();  // 销毁可能存在的物品信息文本框
    }

    public void DestroyInventoryTextBoxGameobject()  // 销毁物品信息文本框的方法
    {
        // 如果创建了库存文本框则销毁它
        if (inventoryTextBoxGameobject != null)  // 检查文本框对象是否存在
        {
            Destroy(inventoryTextBoxGameobject);  // 销毁文本框游戏对象
        }
    }

    // 退出菜单（Esc）时销毁拖拽的对象
    public void DestroyCurrentlyDraggedItems()  // 销毁当前所有拖拽中的物品
    {
        // 循环遍历所有玩家库存物品
        for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)  // 遍历库存列表
        {
            if (inventoryManagementSlot[i].draggedItem != null)  // 检查当前槽位是否有拖拽中的物品
            {
                Destroy(inventoryManagementSlot[i].draggedItem);  // 销毁拖拽物品对象
            }
        }
    }

    private void PopulatePlayerInventory(InventoryLocation inventoryLocation, List<InventoryItem> playerInventoryList)  // 填充玩家库存的方法
    {
        if (inventoryLocation == InventoryLocation.player)  // 检查是否为玩家库存位置
        {
            InitialiseInventoryManagementSlots();  // 初始化库存管理槽位

            // 循环遍历所有玩家库存物品
            for (int i = 0; i < InventoryManager.Instance.inventoryLists[(int)InventoryLocation.player].Count; i++)  // 遍历库存列表
            {
                // 获取库存物品详情
                inventoryManagementSlot[i].itemDetails = InventoryManager.Instance.GetItemDetails(playerInventoryList[i].itemCode);  // 根据物品代码获取详细信息
                inventoryManagementSlot[i].itemQuantity = playerInventoryList[i].itemQuantity;  // 设置物品数量

                if (inventoryManagementSlot[i].itemDetails != null)  // 检查物品详情是否存在
                {
                    // 使用图片和数量更新库存管理槽位
                    inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = inventoryManagementSlot[i].itemDetails.itemSprite;  // 设置物品图标
                    inventoryManagementSlot[i].textMeshProUGUI.text = inventoryManagementSlot[i].itemQuantity.ToString();  // 设置物品数量文本
                }
            }
        }
    }

    private void InitialiseInventoryManagementSlots()  // 初始化库存管理槽位的方法
    {
        // 清空库存槽位
        for (int i = 0; i < Settings.playerMaximumInventoryCapacity; i++)  // 遍历所有槽位容量
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(false);  // 禁用灰色遮罩
            inventoryManagementSlot[i].itemDetails = null;  // 清空物品详情
            inventoryManagementSlot[i].itemQuantity = 0;  // 重置物品数量
            inventoryManagementSlot[i].inventoryManagementSlotImage.sprite = transparent16x16;  // 设置透明图标
            inventoryManagementSlot[i].textMeshProUGUI.text = "";  // 清空数量文本
        }

        // 灰显不可用的槽位
        for (int i = InventoryManager.Instance.inventoryListCapacityIntArray[(int)InventoryLocation.player]; i < Settings.playerMaximumInventoryCapacity; i++)  // 遍历不可用槽位范围
        {
            inventoryManagementSlot[i].greyedOutImageGO.SetActive(true);  // 启用灰色遮罩
        }
    }
}
