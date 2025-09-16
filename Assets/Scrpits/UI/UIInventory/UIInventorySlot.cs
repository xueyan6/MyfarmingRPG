using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera mainCamera;
    private Transform parentItem;
    private GameObject draggedItem;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField] private UIInventoryBar inventoryBar = null; 
    [HideInInspector] public ItemDetails itemDetails;
    [SerializeField] private GameObject itemPrefab = null;
    [HideInInspector] public int itemQuantity;


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
}
