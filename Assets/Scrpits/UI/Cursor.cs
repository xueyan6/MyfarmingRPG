using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    private Canvas canvas;
    private Camera mainCamera;  
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;  
    [SerializeField] private Sprite transparentCursorSprite = null; 
    [SerializeField] private GridCursor gridCursor = null; 

    private bool _cursorIsEnable = false;
    public bool CursorIsEnable { get => _cursorIsEnable; set => _cursorIsEnable = value; }// 光标启用状态属性（公开访问）

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; } // 光标位置有效性属性

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }// 当前选中物品类型属性

    private float _itemUseRadius = 0f; // 非网格使用半径
    public float ItemUseRadius { get => _itemUseRadius; set => _itemUseRadius = value; }// 物品使用半径属性



    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }


    private void Update()
    {
        if (CursorIsEnable)
        {
            DisplayCursor();
        }
    }

    private void DisplayCursor()
    {
        // Get position for cursor获取光标位置
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();// 获取光标世界坐标

        // Set cursor sprite设置光标精灵
        SetCursorValidity(cursorWorldPosition, Player.Instance.GetPlayerCentrePosition());// 设置光标有效性（基于位置）

        // Get rect transform position for cursor获取光标的矩阵变换位置
        cursorRectTransform.position = GetRectTransformPositionForCursor();// 更新光标UI位置
    }

    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition)
    {
        SetCursorToValid();// 默认设为有效状态

        // Check use radius corners检查使用圆角半径
        // 检查是否超出使用范围（四象限边界检查）

        if (
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)//右上
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)//左上
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)//左下
            ||
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)//右下
            )
        {
            SetCursorToInvalid();// 超出范围设为无效
            return;
        }


        // Check item use radius is valid检查物品使用半径是否有效
        // 简单距离检查（X/Y轴分别判断）
        if (Mathf.Abs(cursorPosition.x - playerPosition.x) > ItemUseRadius
            || Mathf.Abs(cursorPosition.y - playerPosition.y) > ItemUseRadius)
        {
            SetCursorToInvalid();
            return;
        }

        // Get selected item details获取当前选中物品详情
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)// 无选中物品时设为无效
        {
            SetCursorToInvalid();
            return;
        }

        // Determine cursor validity based on inventory item selected and what object the cursor is over根据所选库存物品及光标悬停的对象确定光标有效性
        // 根据物品类型特殊处理
        switch (itemDetails.itemType)
        {
            case ItemType.Watering_tool:
            case ItemType.Breaking_tool:
            case ItemType.Chopping_tool:
            case ItemType.Hoeing_tool:
            case ItemType.Reaping_tool:
            case ItemType.Collecting_tool:
                if (!SetCursorValidityTool(cursorPosition, playerPosition, itemDetails))// 工具类特殊验证
                {
                    SetCursorToInvalid();// 验证失败设为无效
                    return;
                }
                break;

            case ItemType.none:
                break;

            case ItemType.count:
                break;

            default:
                break;
        }

    }

    //Set the cursor to be valid设置光标为有效
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;// 切换为绿色光标（有效状态）
        CursorPositionIsValid = true;// 更新状态标记

        gridCursor.DisableCursor(); //禁用网格光标（另外一个cursor不生效，两个不要同时生效）
    }


    // Set the cursor to be invalid将光标设为无效
    private void SetCursorToInvalid()
    {
        cursorImage.sprite = transparentCursorSprite;// 切换为透明光标（无效状态）
        CursorPositionIsValid = false;
        CursorPositionIsValid = false;

        gridCursor.EnableCursor(); //启用网格光标（另外一个cursor生效）
    }



    //Sets the cursor as either valid or invalid for the tool for the target.Returns true if valid or false if invalid
    //将光标设置为对目标工具有效或无效。若有效则返回 true，否则返回 false。
    private bool SetCursorValidityTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        // Switch on tool
        // 根据工具类型进行特殊验证
        switch (itemDetails.itemType)
        {
            case ItemType.Reaping_tool:// 收割工具特殊处理
                return SetCursorValidityReapingTool(cursorPosition, playerPosition, itemDetails);

            default:// 其他工具类型默认返回无效
                return false;
        }
    }

    private bool SetCursorValidityReapingTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails equippedItemDetails)
    {
        List<Item> itemList = new List<Item>();// 创建临时物品列表

        if (HelperMethods.GetComponentsAtCursorLocation<Item>(out itemList, cursorPosition))
        {
            if (itemList.Count != 0)
            {
                foreach (Item item in itemList)// 遍历物品
                {
                    if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)// 验证可收割物
                    {
                        return true; // 发现可收割物立即返回有效
                    }
                }
            }
        }

        return false;
    }


    public void DisableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 0f); // 设置完全透明
        CursorIsEnable = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);// 设置完全不透明
        CursorIsEnable = true;
    }

    public Vector3 GetWorldPositionForCursor()
    {
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);// 获取鼠标屏幕坐标

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);// 转换为世界坐标

        return worldPosition;
    }

    public Vector2 GetRectTransformPositionForCursor()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);// 获取鼠标屏幕坐标

        // 获取鼠标在canvas的rectTransform中的位置
        return RectTransformUtility.PixelAdjustPoint(screenPosition, cursorRectTransform, canvas);// 转换为Canvas坐标系中的精确位置
    }
    //核心架构设计
    //采用状态模式管理光标行为，通过 _CursorIsEnable 和 _CursorPositionIsValid 双重状态控制
    //实现分层验证体系：基础范围检查→物品类型验证→工具专用逻辑
    //依赖注入设计（GridCursor/InventoryManager）
    //坐标转换系统
    //三级坐标体系：屏幕坐标→世界坐标→UI坐标
    //使用Camera.ScreenToWorldPoint处理游戏世界交互
    //通过 RectTransformUtility 处理Canvas精准定位
    //验证逻辑流程
    //    A[初始设为有效] --> B{四象限检查}
    //    B -->|通过| C{轴向距离检查}
    //    B -->|失败| D[设为无效]
    //    C -->|通过| E{物品存在检查}
    //    C -->|失败| D[设为无效]
    //    E -->|存在| F{类型专项验证}
    //    E -->|不存在| D[设为无效]
    //    F -->|通过| G[保持有效]
    //    F -->|失败| D[设为无效]
    //工具类特殊处理
    //收割工具实现二次验证链：
    //获取光标位置所有Item组件
    //筛选Reapable_scenary类型
    //存在至少一个有效目标则验证通过
    //状态同步机制
    //图像切换（green/transparent）与逻辑状态严格绑定
    //网格光标与主光标存在互斥显示关系
    //所有状态变更均通过属性封装器控制
}