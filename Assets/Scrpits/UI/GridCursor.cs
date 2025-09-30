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


    //���ƹ��λ���Ƿ���Ч
    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; }//ͨ��get/set�Զ����Է�װ�������ⲿ���ʺ��޸�

    //������Ʒʹ�÷�Χ�뾶
    private int _itemUseGridRadius = 0;
    public int ItemUseGridRadius { get => _itemUseGridRadius; set => _itemUseGridRadius = value; }

    //��¼��ǰѡ�е���Ʒ����
    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }

    //���ƹ����ʾ״̬�Ŀ���
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
        if (CursorIsEnabled)// �����������ʱִ��
        {
            DisplayCursor();// ��ʾ����߼�
        }
    }

    private Vector3Int DisplayCursor()
    {
        if (grid != null)// ������������Ƿ���Ч
        {
            // Get grid position for cursor��ȡ��������λ��
            Vector3Int gridPosition = GetGridPositionForCursor();

            // Get grid position for player��ȡ��ҵ�����λ��
            Vector3Int playerGridPosition = GetGridPositionForPlayer();

            // Set cursor sprite��֤���λ����Ч��
            SetCursorValidity(gridPosition, playerGridPosition);

            // Get rect transform position for cursor���¹��UIλ��
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

    // ���ù����Ч��״̬
    private void SetCursorValidity(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        SetCursorToValid();// Ĭ����Ϊ��Ч״̬

        // Check item use radius is valid����Ƿ񳬳���Ʒʹ�ð뾶
        if (Mathf.Abs(cursorGridPosition.x - playerGridPosition.x) > ItemUseGridRadius
            || Mathf.Abs(cursorGridPosition.y - playerGridPosition.y) > ItemUseGridRadius)
        {
            SetCursorToInvalid();// �����뾶��Ϊ��Ч
            return;
        }

        // Get selected item details ��ȡ��ǰѡ����Ʒ����
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)// ��ѡ����Ʒ��Ϊ��Ч
        {
            SetCursorToInvalid();
            return;
        }

        // Get grid property details at cursor position��ȡ���λ����������
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        if (gridPropertyDetails != null)
        {
            // Determine cursor validity based on inventory item selected and grid property details������ѡ�����Ŀ��������������ȷ�������Ч��
            //������Ʒ���ͼ����Ч��
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (!IsCursorValidForSeed(gridPropertyDetails))// ��������������
                    {
                        SetCursorToInvalid();
                        return;
                    }
                    break;
                case ItemType.Commodity:
                    if (!IsCursorValidForCommodity(gridPropertyDetails))//��Ʒ����������
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
            SetCursorToInvalid();// ����������Чʱ��Ϊ��Ч
            return;
        }

    }

    //Set the cursor to be invalid���ù��Ϊ��Ч״̬����ɫ��
    private void SetCursorToInvalid()
     {
        cursorImage.sprite = redCursorSprite;
        CursorPositionIsValid = false;
     }


    // Set the cursor to be valid���ù��Ϊ��Ч״̬����ɫ��
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;
        CursorPositionIsValid = true;
    }

    // Test cursor validity for a commodity for the target gridPropertyDetails. Returns true if valid, false if invalid
    //������Ʒ��Ŀ���������������еĹ����Ч�ԡ�����Ч�򷵻� true����Ч�򷵻� false��
    private bool IsCursorValidForCommodity(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;// �����������������Ʒʱ��Ч
    }

    // Set cursor validity for a seed for the target gridPropertyDetails. Returns true if valid, false if invalid
    //����������Ŀ���������������еĹ����Ч�ԡ�����Ч�򷵻� true����Ч�򷵻� false��
    private bool IsCursorValidForSeed(GridPropertyDetails gridPropertyDetails)
    {
        return gridPropertyDetails.canDropItem;// �����������������Ʒʱ��Ч
    }

    //Sets the cursor as either valid or invalid for the tool for target gridPropertyDeails.Returns true if valid or false if invalid
    //���������Ϊ��Ŀ��������������Ĺ�����Ч����Ч������Ч�򷵻� true�����򷵻� false��
    private bool IsCursorValidForTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)// ���巽���������λ���Ƿ��ʺ�ʹ�ù���
    {
        // Switch on tool��������
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridPropertyDetails.isDiggable == true && gridPropertyDetails.daysSinceDug == -1)// ���ؿ��Ƿ���ھ���δ���ھ��
                {
                    #region Need to get any items at location so we can check if they are reapable��Ҫ�ڸõص��ȡ�κ���Ʒ���Ա����Ǽ�������Ƿ���ո
                    // Get world position for cursor��ȡ������������λ��
                    Vector3 cursorWorldPosition = new Vector3(GetWorldPositionForCursor().x + 0.5f, GetWorldPositionForCursor().y + 0.5f, 0f);// �����������������꣨����= +0.5��

                    // Get list of items at cursor location��ȡ���λ�ô�����Ŀ�б�
                    List<Item> itemList = new List<Item>();

                    //��ȡ���λ�ô���������Ʒ���
                    HelperMethods.GetComponentsAtBoxLocation<Item>(out itemList, cursorWorldPosition, Settings.cursorSize, 0f);
                    #endregion

                    // Loop through items found to see if any are reapable type - we are not goint to let the player dig where there are reapable scenary items
                    //�����ҵ�����Ʒ������Ƿ���ڿ��ո����͡������ǲ�����������ڴ��ڿ��ո����Ʒ��λ�ý����ھ�
                    bool foundReapable = false;

                    foreach (Item item in itemList)
                    {
                        if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)// �����Ʒ�Ƿ�Ϊ���ո����Ʒ
                        {
                            foundReapable = true;
                            break;
                        }
                    }

                    if (foundReapable)// ������ڿ��ո���Ʒ
                    {
                        return false;// ������Ч����ֹ�ھ�
                    }
                    else
                    {
                        return true;
                    }

                }
                else
                {
                    return false;// �ؿ鲻���ھ���ѱ��ھ��
                }

            case ItemType.Watering_tool:
                if (gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.daysSinceWatered == -1)//�������Ƿ��ھ���û��ˮ
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

    //����Ŀ�꣺ʵ�ָ���ϵͳ�й����볡����Ʒ�ľ�ȷ�������
    //Э�����̣�
    //���λ�ô���ͨ�� GetWorldPositionForCursor()��ȡ������������0.5ƫ�������뵥Ԫ�����ģ�ȷ���������׼���ǵؿ飩
    //��Ʒ��⣺���� HelperMethods.GetComponentsAtBoxLocation �������������ڵ����пɽ�����Ʒ������ո����Ʒ��
    //�߼����ߣ����ݼ���������Ƿ�����ʹ�ù��ߣ����ֹ�ڴ���Reapable_scenary��Ʒ�ĵؿ��ھ�

    // ���ù�꣨͸������
    public void DisableCursor()
    {
        cursorImage.color = Color.clear;

        CursorIsEnabled = false;
    }

    // ���ù�꣨�ָ���͸����
    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);
        CursorIsEnabled = true;
    }

    // ��ȡ������ڵĹ������������Ӧ������λ��
    public Vector3Int GetGridPositionForCursor()
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        // z is how far the objects are in front of the camera - camera is at -10 so objects are (-)-10 in front = 10
        // z ��ʾ��������������Զ�����������λ�� -10 �����������λ�� (-)-10 ��������������� 10 ��λ

        return grid.WorldToCell(worldPosition);// ����������ת��Ϊ��������
    }

    public Vector3Int GetGridPositionForPlayer()
    {
        return grid.WorldToCell(Player.Instance.transform.position);// ת�������������Ϊ��������
    }

    //������ƶ����ʱ���ú����������������λ��׼ȷӳ�䵽��ĻUIλ�ã�ʹ����ܹ���ȷ��ʾ�ڶ�Ӧ����������ϡ�
    public Vector2 GetRectTransformPositionForCursor(Vector3Int gridPosition)
    {
        Vector3 gridWorldPosition = grid.CellToWorld(gridPosition);// ��������ת��������
        Vector2 gridScreenPosition = mainCamera.WorldToScreenPoint(gridWorldPosition);// ����3D����ת��Ļ2D����
        return RectTransformUtility.PixelAdjustPoint(gridScreenPosition, cursorRectTransform, canvas);// ����UI����������ֱ�������Ӧ����
    }

    public Vector3 GetWorldPositionForCursor()
    {
        return grid.CellToWorld(GetGridPositionForCursor());
    }


    //�ܽ᣺
    //��δ���ʵ����һ����������ϵͳ�Ľ�������������������߼��ɷ�Ϊ�����Σ�
    //״̬�����
    //��װ�����Ч��(_cursorPositionIsValid)����Ʒʹ�ð뾶(_itemUseGridRadius)��״̬
    //ͨ�������ṩ��ȫ���ʿ��ƣ���CursorIsEnabled���ƹ������
    //�������ڹ����
    //OnEnable/OnDisable�����������¼�����
    //Start������ʼ�������(mainCamera)�ͻ���(canvas)����
    //���������
    //Update����ÿ֡���λ�ø���(DisplayCursor)
    //����������ҵ������������
    //������Ʒ����(SelectedItemType)��֤������Ч��
    //�Ӿ�������
    //ͨ����/�̹��Sprite�л���ʾ��Ч��
    //͸���ȿ���ʵ�ֹ�������Ч��
    //����ת����
    //ʵ����Ļ���������������������������ת��
    //�ṩ���λ�ú͹��λ�õ�����ӳ�䷽��
    //���Ľ������̣�
    //��ȡ��ǰ������ҵ���������
    //�������Ƿ���ItemUseGridRadius��Χ��
    //����ѡ����Ʒ����ִ���ض���֤
    //���¹���Ӿ�״̬����Ч�Ա��
    //���⴦��
    //����(Seed)����Ʒ(Commodity)������Ʒ�ж�������Ч�Լ��
    //��������(GridPropertyDetails)�����Ƿ����������Ʒ
    //ͨ���¼�ϵͳʵ�ֶ�̬��������
}