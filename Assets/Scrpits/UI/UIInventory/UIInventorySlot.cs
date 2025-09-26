using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler,IPointerClickHandler
{
    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GridCursor gridCursor;
    private GameObject draggedItem;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public ItemDetails itemDetails;
    [SerializeField] private GameObject itemPrefab = null;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;


    private void Awake()
    {
        parentCanvas= GetComponentInParent<Canvas>();
    }

    //在切换场景的过程中，我们并不能总是找到他们（FindGameObjectWithTag）。因为场景加载需要时间，可能场景还没加载完成就在取值了
    //所以需要订阅加载完成的事件，在事件中处理元素的获取。
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
    }


    private void Start()
    {
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
    }
    //获取主相机和物品父对象引用
    //为后续的屏幕坐标转换和物品生成做准备

    private void ClearCursors()
    {
        // Disable cursor禁用光标
        gridCursor.DisableCursor();

        // Set item type to none将道具类型设置为无
        gridCursor.SelectedItemType = ItemType.none;

    }

        // Set this inventory slot item to be selected将此库存槽位物品设为选中状态
        private void SetSelectedItem()
    {
        // Clear currently highlighted items清除当前选中的项目
        inventoryBar.ClearHighlightOnInventorySlots();

        // Highlight item on inventory bar在库存栏中突出显示物品
        isSelected = true;

        // Set highlighted inventory slots设置高亮库存槽位
        inventoryBar.SetHighlightedInventorySlots();


        // Set use radius for cursors 设置光标使用半径 
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;

        // If item requires a grid cursor then enable cursor如果项目需要网格光标，则启用光标
        if (itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        // Set item type设置道具类型
        gridCursor.SelectedItemType = itemDetails.itemType;


        // Set item selected in inventory在物品栏中选定装备
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);


        if (itemDetails.canBeCarried == true)
        {
            // Show player carrying item显示玩家携带的物品
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        }
        else
        {
            Player.Instance.ClearCarriedItem();
        }

    }

    private void ClearSelectedItem()
    {
        ClearCursors();

        // Clear currently highlighted item清除当前选中的项目
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        // set no item selected in inventory在物品栏中未选择任何物品
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);

        // Clear player carrying item清除玩家携带物品
        Player.Instance.ClearCarriedItem();
    }


        //Drops the item (if selected) at the current mouse position. Called by the DropItem event.在当前鼠标位置丢弃该项目（若已选中）。由DropItem事件调用。
        private void DropSelectedItemAtMousePosition()
        {
            if(itemDetails != null && isSelected)
            {
                //If a valid cursor position如果光标位置有效
                if (gridCursor.CursorPositionIsValid)
                {
                    Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

                    // Create item from prefab at mouse position在鼠标位置处从预制件创建物品
                    GameObject itemGameObject = Instantiate(itemPrefab, new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize / 2f, worldPosition.z),Quaternion.identity, parentItem);
                    Item item = itemGameObject.GetComponent<Item>();
                    item.ItemCode = itemDetails.itemCode;

                    //Remove item from players inventory从玩家背包中移除物品
                    InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);


                    // If no more of item then clear selected若无更多项目则清除选中项
                    if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                    {
                        ClearSelectedItem();
                    }

                }
                 

                


            }
        }
    //实现将物品从库存中移除并在世界场景中生成
    //使用屏幕坐标转换确保物品出现在正确位置

    public void OnBeginDrag(PointerEventData eventData)//拖拽开始
    {
        if(itemDetails != null)
        {
            //Disable keyboard input禁用键盘输入
            Player.Instance.DisablePlayerInputAndResetMovement();

            //Instatiate gameobject as dragged item将游戏对象初始化为拖拽项
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Get image for dargged item获取拖动项的图像
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            SetSelectedItem();
        }
    }
    //创建拖拽物品的视觉表示
    //禁用玩家控制以避免冲突


    public void OnDrag(PointerEventData eventData)//拖拽中
    {
        // Move gameobject as dragged item将游戏对象作为拖拽项移动
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }
    //实现拖拽物品的实时位置更新


    public void OnEndDrag(PointerEventData eventData)//拖拽结束
    {
        //Destory gameobject as dragged item销毁作为拖拽项的游戏对象
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            //If drag ends over inventory bar, get item drag is over and swap them如果拖拽结束在物品栏上方，则完成拖拽操作并交换物品
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                // get the slot number where the drag ended获取拖动结束的位置槽号
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent < UIInventorySlot > ().slotNumber;

                // Swap inventory items in inventory list 在库存列表中交换物品
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                // Destroy inventory text box销毁库存文本框
                DestroyInventoryTextBox();

                //Clear Selected Item
                ClearSelectedItem();
            }

            //else attempt to drop the item if it can be dropped 否则尝试丢弃该物品（如果可以丢弃的话） 
            else
            {
                if(itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }
            //处理拖拽结束后的多种情况
            //实现物品丢弃或放回的逻辑分支


            //Enable player input启用玩家输入
            Player.Instance.EnablePlayerInput();
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        // if left click如果左键点击
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if inventory slot currently selected then deselect如果当前选中了库存栏位，则取消选中
            if (isSelected == true)
            {
                ClearSelectedItem();
            }
            else  //未被选中且有东西则显示选中的效果
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }



    public void OnPointerEnter(PointerEventData eventData)//当指针（鼠标或触摸点）进入UI元素的矩形边界时触发该事件
    {
        // Populate text box with item details 在文本框中填充商品详情 
        if (itemQuantity != 0)//仅当物品数量>0时显示详情
        {
            // Instantiate inventory text box 实例化物品栏文本框
            inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);// 实例化预制体文本框（预制体资源、基于当前物体位置、无旋转）
            inventoryBar.inventoryTextBoxGameobject .transform .SetParent(parentCanvas.transform, false);// SetParent将文本框挂载到父级Canvas（Unity的UI系统通过Canvas组织渲染层级，所有UI元素必须直接或间接挂载到Canvas下才能被正确渲染。类似"只有贴在画布上的颜料才会被看到"。）
                                                                                                         // false表示‌不保持世界坐标。（文本框的局部坐标（Local Position）会相对于Canvas重新计算；如果设为true，文本框会保持原世界坐标，可能导致位置错乱）
            // 获取文本框组件 
            UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            //Set item type description 设置物品类型描述 
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            //Populate text box 填充文本框 
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");//（物品名称，类型描述，预留字段，详细说明，预留字段，预留字段）

            //Set text box position according to inventory bar position 根据物品栏位置设置文本框位置
            if (inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
                //y轴动态偏移50像素，能有效防止文本框遮挡当前悬停的UI元素或相邻交互区域，确保用户始终可见完整信息；
                //可兼容物品栏在屏幕顶部或底部的场景。这种设计保证文本框始终出现在物品栏的"外侧"，符合用户对提示框位置的心理预期
                
            }
            
        }
    }

    public void OnPointerExit(PointerEventData eventData)//当指针（鼠标或触摸点）离开UI元素的矩形边界时触发该事件
    {
      DestroyInventoryTextBox();
    }

    public void DestroyInventoryTextBox()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }
    
}



