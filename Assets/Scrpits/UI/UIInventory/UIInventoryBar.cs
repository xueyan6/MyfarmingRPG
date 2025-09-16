using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    [SerializeField] private Sprite blank16x16sprite = null;
    [SerializeField] private UIInventorySlot[] inventorySlot = null;
    public GameObject inventoryBarDraggedItem;//库存条拖动项
    [HideInInspector] public GameObject inventoryTextBoxGameobject;



    private RectTransform rectTransform;

    private bool _isInventoryBarPositionBottom = true;

    public bool IsInventoryBarPositionBottom { get => _isInventoryBarPositionBottom; set => _isInventoryBarPositionBottom = value; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    //Update is called once per frame更新每帧调用一次
    private void Update()
    {
        //Switch inventory bar position depending on player position根据玩家位置切换道具栏位置
        SwitchInventoryBarPosition();
    }

    private void ClearInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            //loop through inventory slots and update with blank sprite循环遍历物品栏格位并更新为空白贴图
            for (int i = 0; i < inventorySlot.Length; i++)

            {
                inventorySlot[i].inventorySlotImage.sprite = blank16x16sprite;
                inventorySlot[i].textMeshProUGUI.text = "";
                inventorySlot[i].itemDetails = null;
                inventorySlot[i].itemQuantity = 0;
            }
        }
    }


    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if(inventoryLocation==InventoryLocation.player)
        {
            ClearInventorySlots();

            if(inventorySlot.Length > 0 && inventoryList.Count>0)
            {
                //loop through inventory slots and update with corresponding inventory list item遍历库存槽位，并更新为对应的库存列表项
                for(int i=0;i<inventorySlot.Length;i++)
                {
                    if (i < inventoryList.Count)
                    {
                        int itemCode = inventoryList[i].itemCode;

                        //ItemDetails itemDetails = InventoryManager.Instance.itemList.itemDetails.Find(x => x.itemCode == itemCode);
                        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                        if(itemDetails != null)
                        {
                            //add images and details to inventory item slot为库存物品栏添加图片和详细信息
                            inventorySlot[i].inventorySlotImage.sprite = itemDetails.itemSprite;
                            inventorySlot[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                            inventorySlot[i].itemDetails = itemDetails;
                            inventorySlot[i].itemQuantity = inventoryList[i].itemQuantity;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
    //总结:上述两段代码（ClearInventorySlots，InventoryUpdated）共同构成了一个物品栏管理系统，主要功能包括：
    //1.清空物品栏所有格位（视觉和数据的双重清理）2.根据库存数据更新物品栏显示 3.实现玩家物品栏的动态刷新

    //ClearInventorySlots功能说明：1.清空所有物品栏格位的视觉效果和关联数据 2.执行操作前会检查物品栏数组是否初始化
    //关键操作：1.将每个格位的贴图设置为空白贴图(blank16x16sprite) 2.清除数量显示文本 3.清空物品详情引用 4.重置物品数量为0.
    //在游戏开发中，先清空物品栏所有格位再更新是常见的优化策略，主要原因包括：
    //1.状态一致性保证‌：
    //防止旧数据残留（如被移除的物品仍显示）
    //确保UI与底层数据完全同步
    //2.动态列表处理‌：
    //当新物品列表比旧列表短时，避免末尾格位残留旧物品
    //统一处理物品数量变化（如合并堆叠后的空位）
    //3.性能优化‌：
    //批量清空操作比逐个对比删除更高效
    //减少条件判断逻辑（无需单独处理每个格位的更新状态）
    //4.动画/效果触发‌：
    //为后续物品添加动画提供干净的初始状态
    //方便实现整体刷新时的过渡效果
    //5.错误预防‌：
    //避免因索引错位导致的显示异常
    //防止物品拖动等交互操作引发引用错误
    //这种"先破后立"的方式虽然看似多了一次遍历，但实际比复杂的差异对比算法更可靠高效，是游戏UI开发的经典模式。

    //ClearInventorySlots涉及到游戏库存系统的更新机制设计原理。实际上，这种"先清空后重建"的方式不会导致已有物品丢失，原因如下：
    //1.数据源分离原则‌：
    //清空操作只针对UI显示层（inventorySlot数组）
    //真实物品数据存储在独立的inventoryList中
    //2.更新时序控制‌：
    //InventoryUpdate->ClearSlots: 1.清空UI显示
    //ClearSlots->RebuildSlots: 2.从数据源重建
    //RebuildSlots->InventoryList: 3.读取最新数据
    //3.数据流向保障‌：
    //原始数据：InventoryManager维护的全局物品库
    //中间数据：作为参数传入的inventoryList副本
    //显示数据：临时构建的UI状态
    //4.异常处理机制‌：
    //在GetItemDetails调用失败时会跳过该槽位
    //循环以inventoryList.Count为安全边界
    //这种设计模式在Unity游戏开发中非常常见，既能保证UI响应速度，又能通过数据层的分离确保核心数据安全。

    //InventoryUpdated方法详解：
    //1.功能说明：
    //响应库存更新事件，只处理玩家物品栏的更新
    //先清空后重建物品栏内容
    //2.关键操作：
    //先调用ClearInventorySlots清空所有格位
    //遍历物品栏格位和库存列表
    //通过物品代码(itemCode)查询物品详情
    //更新格位的贴图、数量显示和数据引用

    private void SwitchInventoryBarPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();

        if(playerViewportPosition.y > 0.3f&&IsInventoryBarPositionBottom==false )
        {
            //transform.position = new Vector3(transform.position.x, 7.5f, 0f); // this was changed to control the recttransform see below此处修改用于控制矩形变换体，详见下文
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if(playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            //transform.position = new Vector3(transform.position.x, mainCamera.pixelHeight - 120f, 0f); // this was changed to control the recttransform see below此处修改用于控制矩形变换体，详见下文
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionBottom = false;
        }
    }
    
}
