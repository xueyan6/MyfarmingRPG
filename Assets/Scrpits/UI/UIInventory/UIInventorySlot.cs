using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GameObject draggedItem;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private UIInventoryBar inventoryBar = null;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [HideInInspector] public ItemDetails itemDetails;
    [SerializeField] private GameObject itemPrefab = null;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;

    private void Awake()
    {
        parentCanvas= GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }
    //获取主相机和物品父对象引用
    //为后续的屏幕坐标转换和物品生成做准备

    //Drops the item (if selected) at the current mouse position. Called by the DropItem event.在当前鼠标位置丢弃该项目（若已选中）。由DropItem事件调用。
    private void DropSelectedItemAtMousePosition()
    {
        if(itemDetails != null)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

            // Create item from prefab at mouse position在鼠标位置处从预制件创建物品
            GameObject itemGameObject = Instantiate(itemPrefab, worldPosition, Quaternion.identity, parentItem);
            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = itemDetails.itemCode;

            //Remove item from players inventory从玩家背包中移除物品
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);
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
}
