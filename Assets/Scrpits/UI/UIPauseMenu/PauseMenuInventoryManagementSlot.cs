using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class PauseMenuInventoryManagementSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image inventoryManagementSlotImage;  // 显示槽位中物品图标的Image组件
    public TextMeshProUGUI textMeshProUGUI;  // 显示物品数量的文本组件
    public GameObject greyedOutImageGO;  // 物品不可用时的灰色遮罩对象
    [SerializeField] private PauseMenuInventoryManagement inventoryManagement = null;  // 库存管理系统引用
    [SerializeField] private GameObject inventoryTextBoxPrefab = null;  // 物品信息提示框预制体

    [HideInInspector] public ItemDetails itemDetails; // 当前槽位存储的物品详细信息
    [HideInInspector] public int itemQuantity; // 当前槽位中物品的数量
    [SerializeField] private int slotNumber = 0; // 槽位的唯一标识编号，范围0~47

    public GameObject draggedItem;  // 正在被拖拽的物品对象
    private Canvas parentCanvas; // 父级Canvas引用，用于UI层级管理

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();  // 初始化时获取父Canvas组件
    }

    public void OnBeginDrag(PointerEventData eventData)  // 开始拖拽事件
    {
        if (itemQuantity != 0)  // 只有槽位中有物品时才允许拖拽
        {
            // 实例化拖拽时显示的物品对象
            draggedItem = Instantiate(inventoryManagement.inventoryManagementDraggedItemPrefab, inventoryManagement.transform);

            // 获取拖拽物品的图像组件
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventoryManagementSlotImage.sprite;  // 设置拖拽物品的图标
        }
    }

    public void OnDrag(PointerEventData eventData)  // 拖拽过程中事件
    {
        // 移动拖拽物体跟随鼠标位置
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;  // 更新拖拽物体位置到鼠标位置
        }
    }

    public void OnEndDrag(PointerEventData eventData)  // 结束拖拽事件
    {
        // 销毁拖拽物体
        if (draggedItem != null)
        {
            Destroy(draggedItem);  // 移除拖拽时显示的物品对象

            // 检查拖拽结束位置是否有有效对象
            if (eventData.pointerCurrentRaycast.gameObject != null
                && eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>() != null)
            {
                // 获取目标槽位的编号
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<PauseMenuInventoryManagementSlot>().slotNumber;

                // 在库存列表中交换物品位置
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

                // 销毁可能存在的物品信息提示框
                inventoryManagement.DestroyInventoryTextBoxGameobject();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)  // 鼠标悬停进入事件
    {
        // 当槽位中有物品时显示详细信息
        if (itemQuantity != 0)
        {
            // 实例化物品信息提示框
            inventoryManagement.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
            inventoryManagement.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);  // 设置父级为Canvas

            // 获取提示框的文本组件
            UIInventoryTextBox inventoryTextBox = inventoryManagement.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            // 从库存管理器获取物品类型描述
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

            // 填充提示框的文本内容
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

            // 根据槽位位置设置提示框显示位置
            if (slotNumber > 23)  // 如果是下半部分的槽位
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);  // 设置轴心点
                inventoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);  // 在槽位上方显示
            }
            else  // 如果是上半部分的槽位
            {
                inventoryManagement.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);  // 设置轴心点
                inventoryManagement.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);  // 在槽位下方显示
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)  // 鼠标悬停离开事件
    {
        inventoryManagement.DestroyInventoryTextBoxGameobject();  // 销毁物品信息提示框
    }
}
