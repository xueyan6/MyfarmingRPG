using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCursor : MonoBehaviour
{
    private Canvas canvas;
    private Grid grid;
    private Camera mainCamera;
    [SerializeField] private Image cursorImage = null;
    [SerializeField] private RectTransform cursorRectTransform = null;
    [SerializeField] private Sprite greenCursorSprite = null;
    [SerializeField] private Sprite redCursorSprite = null;


    //控制光标位置是否有效
    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }//通过get/set自动属性封装，便于外部访问和修改

    //定义物品使用范围半径
    private int _itemUseGridRadius = 0;
    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }

    //记录当前选中的物品类型
    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    //控制光标显示状态的开关
    private bool _cursorIsEnabled = false;
    public bool CursorIsEnabled { get => _cursorIsEnabled; set => _cursorIsEnabled = value; }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (CursorIsEnabled)// 仅当光标启用时执行
        {
            DisplayCursor();// 显示光标逻辑
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)// 检查网格引用是否有效
        {
            // Get grid position for cursor获取光标的网格位置
            Vector3Int gridPosition = GetGridPositionForCursor();

            // Get grid position for player获取玩家的网格位置
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            // Set cursor sprite验证光标位置有效性
            SetCursorValidity(gridPosition, playerGridPosition);

            // Get rect transform position for cursor更新光标UI位置
            cursorRectTransform.position = GetRectTransformPositionForCursor(gridPosition);

            return gridPosition;
        }
        else
        {
            return Vector3Int.zero;
        }
    }

    private void SceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
    }

    // 设置光标有效性状态
    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();// 默认设为有效状态

        // Check item use radius is valid检查是否超出物品使用半径
        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();// 超出半径设为无效
            return;
        }

        // Get selected item details 获取当前选中物品详情
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)// 无选中物品设为无效
        {
            SetCursorToInvalid();
            return;
        }

        // Get grid property details at cursor position获取光标位置网格属性
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            // Determine cursor validity based on inventory item selected and grid property details根据所选库存项目和网格属性详情确定光标有效性
            //根据物品类型检查有效性
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))// 种子类型特殊检查
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))//商品类型特殊检查
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;

                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                    if (!IsCursorValidForTool(gridPropertyDetails, itemDetails))
                    {
                        SetCursorToInvalid();
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
        else
        {
            SetCursorToInvalid();// 网格属性无效时设为无效
            return;
        }

    }

    //Set the cursor to be invalid设置光标为无效状态（红色）
    private void SetCursorToInvalid()
     {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
     }


    // Set the cursor to be valid设置光标为有效状态（绿色）
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    // Test cursor validity for a commodity for the target gridPropertyDetails. Returns true if valid, false if invalid
    //测试商品在目标网格属性详情中的光标有效性。若有效则返回 true，无效则返回 false。
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;// 仅当网格允许放置物品时有效
    }

    // Set cursor validity for a seed for the target gridPropertyDetails. Returns true if valid, false if invalid
    //测试种子在目标网格属性详情中的光标有效性。若有效则返回 true，无效则返回 false。
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;// 仅当网格允许放置物品时有效
    }

    //Sets the cursor as either valid or invalid for the tool for target gridPropertyDeails.Returns true if valid or false if invalid
    //将光标设置为对目标网格属性详情的工具有效或无效。若有效则返回 true，否则返回 false。
    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)// 定义方法：检查光标位置是否适合使用工具
    {
        // Switch on tool开启工具
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridPropertyDetails.isDiggable == true && gridPropertyDetails.daysSinceDug == -1)// 检查地块是否可挖掘且未被挖掘过
                {
                    #region Need to get any items at location so we can check if they are reapable需要在该地点获取任何物品，以便我们检查它们是否可收割。
                    // Get world position for cursor获取光标的世界坐标位置
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);// 计算光标中心世界坐标（中心= +0.5）

                    // Get list of items at cursor location获取光标位置处的项目列表
                    List<Item> itemList = new List<Item>();

                    //获取光标位置处的所有物品组件
                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);
                    #endregion

                    // Loop through items found to see if any are reapable type - we are not goint to let the player dig where there are reapable scenary items
                    //遍历找到的物品，检查是否存在可收割类型——我们不会允许玩家在存在可收割场景物品的位置进行挖掘。
                    bool foundReapable = false;

                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)// 检查物品是否为可收割场景物品
                        {
                            foundReapable = true;
                            break;
                        }
                    }

                    if (foundReapable)// 如果存在可收割物品
                    {
                        return false;// 返回无效（禁止挖掘）
                    }
                    else
                    {
                        return true;
                    }

                }
                else
                {
                    return false;// 地块不可挖掘或已被挖掘过
                }

            case ItemType.Watering_tool:
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)//检查地面是否被挖掘并且没浇水
                {
                    return true;
                }
                else
                {
                    return false;
                }


                default:
                return false;

        }

    }

    //核心目标：实现耕作系统中工具与场景物品的精确交互检测
    //协作流程：
    //光标位置处理：通过 GetWorldPositionForCursor()获取网格坐标后，添加0.5偏移量对齐单元格中心（确保检测区域精准覆盖地块）
    //物品检测：调用 HelperMethods.GetComponentsAtBoxLocation 检测光标矩形区域内的所有可交互物品（如可收割场景物品）
    //逻辑决策：根据检测结果决定是否允许使用工具（如禁止在存在Reapable_scenary物品的地块挖掘）

    // 禁用光标（透明化）
    public void DisableCursor()
    {
        cursorImage.color = Color.clear;

        CursorIsEnabled = false;
    }

    // 启用光标（恢复不透明）
    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    // 获取鼠标所在的光标的世界坐标对应的网格位置
    public Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        // z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front = 10
        // z 表示物体距离摄像机的远近——摄像机位于 -10 处，因此物体位于 (-)-10 处，即距离摄像机 10 单位

        return grid.WorldToCell(worldPosition);// 将世界坐标转换为网格坐标
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);// 转换玩家世界坐标为网格坐标
    }

    //当玩家移动鼠标时，该函数将计算出的网格位置准确映射到屏幕UI位置，使光标能够正确显示在对应的网格格子上。
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);// 网格坐标转世界坐标
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);// 世界3D坐标转屏幕2D坐标
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);// 适配UI画布，解决分辨率自适应问题
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }


    //总结：
    //这段代码实现了一个基于网格系统的交互光标控制器，其核心逻辑可分为五个层次：
    //状态管理层
    //封装光标有效性(_cursorPositionIsValid)、物品使用半径(_itemUseGridRadius)等状态
    //通过属性提供安全访问控制，如CursorIsEnabled控制光标显隐
    //生命周期管理层
    //OnEnable/OnDisable管理场景加载事件订阅
    //Start方法初始化摄像机(mainCamera)和画布(canvas)引用
    //交互处理层
    //Update驱动每帧光标位置更新(DisplayCursor)
    //计算光标与玩家的网格坐标距离
    //根据物品类型(SelectedItemType)验证交互有效性
    //视觉反馈层
    //通过红/绿光标Sprite切换显示有效性
    //透明度控制实现光标的显隐效果
    //坐标转换层
    //实现屏幕坐标→世界坐标→网格坐标的三级转换
    //提供玩家位置和光标位置的网格映射方法
    //核心交互流程：
    //获取当前光标和玩家的网格坐标
    //检查距离是否在ItemUseGridRadius范围内
    //根据选中物品类型执行特定验证
    //更新光标视觉状态和有效性标记
    //特殊处理：
    //种子(Seed)和商品(Commodity)类型物品有独立的有效性检查
    //网格属性(GridPropertyDetails)决定是否允许放置物品
    //通过事件系统实现动态场景加载
}