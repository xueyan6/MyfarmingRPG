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
    //��ȡ���������Ʒ����������
    //Ϊ��������Ļ����ת������Ʒ������׼��

    //Drops the item (if selected) at the current mouse position. Called by the DropItem event.�ڵ�ǰ���λ�ö�������Ŀ������ѡ�У�����DropItem�¼����á�
    private void DropSelectedItemAtMousePosition()
    {
        if(itemDetails != null)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));

            // Create item from prefab at mouse position�����λ�ô���Ԥ�Ƽ�������Ʒ
            GameObject itemGameObject = Instantiate(itemPrefab, worldPosition, Quaternion.identity, parentItem);
            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = itemDetails.itemCode;

            //Remove item from players inventory����ұ������Ƴ���Ʒ
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);
        }
    }
    //ʵ�ֽ���Ʒ�ӿ�����Ƴ��������糡��������
    //ʹ����Ļ����ת��ȷ����Ʒ��������ȷλ��

    public void OnBeginDrag(PointerEventData eventData)//��ק��ʼ
    {
        if(itemDetails != null)
        {
            //Disable keyboard input���ü�������
            Player.Instance.DisablePlayerInputAndResetMovement();

            //Instatiate gameobject as dragged item����Ϸ�����ʼ��Ϊ��ק��
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            //Get image for dargged item��ȡ�϶����ͼ��
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;
        }
    }
    //������ק��Ʒ���Ӿ���ʾ
    //������ҿ����Ա����ͻ


    public void OnDrag(PointerEventData eventData)//��ק��
    {
        // Move gameobject as dragged item����Ϸ������Ϊ��ק���ƶ�
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }
    //ʵ����ק��Ʒ��ʵʱλ�ø���


    public void OnEndDrag(PointerEventData eventData)//��ק����
    {
        //Destory gameobject as dragged item������Ϊ��ק�����Ϸ����
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            //If drag ends over inventory bar, get item drag is over and swap them�����ק��������Ʒ���Ϸ����������ק������������Ʒ
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {

            }
            //else attempt to drop the item if it can be dropped �����Զ�������Ʒ��������Զ����Ļ��� 
            else
            {
                if(itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }
            //������ק������Ķ������
            //ʵ����Ʒ������Żص��߼���֧


            //Enable player input�����������
            Player.Instance.EnablePlayerInput();
        }
    }
}
