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
    public bool CursorIsEnable { get => _cursorIsEnable; set => _cursorIsEnable = value; }// �������״̬���ԣ��������ʣ�

    private bool _cursorPositionIsValid = false;
    public bool CursorPositionIsValid { get => _cursorPositionIsValid; set => _cursorPositionIsValid = value; } // ���λ����Ч������

    private ItemType _selectedItemType;
    public ItemType SelectedItemType { get => _selectedItemType; set => _selectedItemType = value; }// ��ǰѡ����Ʒ��������

    private float _itemUseRadius = 0f; // ������ʹ�ð뾶
    public float ItemUseRadius { get => _itemUseRadius; set => _itemUseRadius = value; }// ��Ʒʹ�ð뾶����



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
        // Get position for cursor��ȡ���λ��
        Vector3 cursorWorldPosition = GetWorldPositionForCursor();// ��ȡ�����������

        // Set cursor sprite���ù�꾫��
        SetCursorValidity(cursorWorldPosition, Player.Instance.GetPlayerCentrePosition());// ���ù����Ч�ԣ�����λ�ã�

        // Get rect transform position for cursor��ȡ���ľ���任λ��
        cursorRectTransform.position = GetRectTransformPositionForCursor();// ���¹��UIλ��
    }

    private void SetCursorValidity(Vector3 cursorPosition, Vector3 playerPosition)
    {
        SetCursorToValid();// Ĭ����Ϊ��Ч״̬

        // Check use radius corners���ʹ��Բ�ǰ뾶
        // ����Ƿ񳬳�ʹ�÷�Χ�������ޱ߽��飩

        if (
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)//����
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y > (playerPosition.y + ItemUseRadius / 2f)//����
            ||
            cursorPosition.x < (playerPosition.x - ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)//����
            ||
            cursorPosition.x > (playerPosition.x + ItemUseRadius / 2f) && cursorPosition.y < (playerPosition.y - ItemUseRadius / 2f)//����
            )
        {
            SetCursorToInvalid();// ������Χ��Ϊ��Ч
            return;
        }


        // Check item use radius is valid�����Ʒʹ�ð뾶�Ƿ���Ч
        // �򵥾����飨X/Y��ֱ��жϣ�
        if (Mathf.Abs(cursorPosition.x - playerPosition.x) > ItemUseRadius
            || Mathf.Abs(cursorPosition.y - playerPosition.y) > ItemUseRadius)
        {
            SetCursorToInvalid();
            return;
        }

        // Get selected item details��ȡ��ǰѡ����Ʒ����
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails == null)// ��ѡ����Ʒʱ��Ϊ��Ч
        {
            SetCursorToInvalid();
            return;
        }

        // Determine cursor validity based on inventory item selected and what object the cursor is over������ѡ�����Ʒ�������ͣ�Ķ���ȷ�������Ч��
        // ������Ʒ�������⴦��
        switch (itemDetails.itemType)
        {
            case ItemType.Watering_tool:
            case ItemType.Breaking_tool:
            case ItemType.Chopping_tool:
            case ItemType.Hoeing_tool:
            case ItemType.Reaping_tool:
            case ItemType.Collecting_tool:
                if (!SetCursorValidityTool(cursorPosition, playerPosition, itemDetails))// ������������֤
                {
                    SetCursorToInvalid();// ��֤ʧ����Ϊ��Ч
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

    //Set the cursor to be valid���ù��Ϊ��Ч
    private void SetCursorToValid()
    {
        cursorImage.sprite = greenCursorSprite;// �л�Ϊ��ɫ��꣨��Ч״̬��
        CursorPositionIsValid = true;// ����״̬���

        gridCursor.DisableCursor(); //���������꣨����һ��cursor����Ч��������Ҫͬʱ��Ч��
    }


    // Set the cursor to be invalid�������Ϊ��Ч
    private void SetCursorToInvalid()
    {
        cursorImage.sprite = transparentCursorSprite;// �л�Ϊ͸����꣨��Ч״̬��
        CursorPositionIsValid = false;
        CursorPositionIsValid = false;

        gridCursor.EnableCursor(); //���������꣨����һ��cursor��Ч��
    }



    //Sets the cursor as either valid or invalid for the tool for the target.Returns true if valid or false if invalid
    //���������Ϊ��Ŀ�깤����Ч����Ч������Ч�򷵻� true�����򷵻� false��
    private bool SetCursorValidityTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails itemDetails)
    {
        // Switch on tool
        // ���ݹ������ͽ���������֤
        switch (itemDetails.itemType)
        {
            case ItemType.Reaping_tool:// �ո�����⴦��
                return SetCursorValidityReapingTool(cursorPosition, playerPosition, itemDetails);

            default:// ������������Ĭ�Ϸ�����Ч
                return false;
        }
    }

    private bool SetCursorValidityReapingTool(Vector3 cursorPosition, Vector3 playerPosition, ItemDetails equippedItemDetails)
    {
        List<Item> itemList = new List<Item>();// ������ʱ��Ʒ�б�

        if (HelperMethods.GetComponentsAtCursorLocation<Item>(out itemList, cursorPosition))
        {
            if (itemList.Count != 0)
            {
                foreach (Item item in itemList)// ������Ʒ
                {
                    if (InventoryManager.Instance.GetItemDetails(item.ItemCode).itemType == ItemType.Reapable_scenary)// ��֤���ո���
                    {
                        return true; // ���ֿ��ո�������������Ч
                    }
                }
            }
        }

        return false;
    }


    public void DisableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 0f); // ������ȫ͸��
        CursorIsEnable = false;
    }

    public void EnableCursor()
    {
        cursorImage.color = new Color(1f, 1f, 1f, 1f);// ������ȫ��͸��
        CursorIsEnable = true;
    }

    public Vector3 GetWorldPositionForCursor()
    {
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);// ��ȡ�����Ļ����

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);// ת��Ϊ��������

        return worldPosition;
    }

    public Vector2 GetRectTransformPositionForCursor()
    {
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);// ��ȡ�����Ļ����

        // ��ȡ�����canvas��rectTransform�е�λ��
        return RectTransformUtility.PixelAdjustPoint(screenPosition, cursorRectTransform, canvas);// ת��ΪCanvas����ϵ�еľ�ȷλ��
    }
    //���ļܹ����
    //����״̬ģʽ��������Ϊ��ͨ�� _CursorIsEnable �� _CursorPositionIsValid ˫��״̬����
    //ʵ�ֲַ���֤��ϵ��������Χ������Ʒ������֤������ר���߼�
    //����ע����ƣ�GridCursor/InventoryManager��
    //����ת��ϵͳ
    //����������ϵ����Ļ��������������UI����
    //ʹ��Camera.ScreenToWorldPoint������Ϸ���罻��
    //ͨ�� RectTransformUtility ����Canvas��׼��λ
    //��֤�߼�����
    //    A[��ʼ��Ϊ��Ч] --> B{�����޼��}
    //    B -->|ͨ��| C{���������}
    //    B -->|ʧ��| D[��Ϊ��Ч]
    //    C -->|ͨ��| E{��Ʒ���ڼ��}
    //    C -->|ʧ��| D[��Ϊ��Ч]
    //    E -->|����| F{����ר����֤}
    //    E -->|������| D[��Ϊ��Ч]
    //    F -->|ͨ��| G[������Ч]
    //    F -->|ʧ��| D[��Ϊ��Ч]
    //���������⴦��
    //�ո��ʵ�ֶ�����֤����
    //��ȡ���λ������Item���
    //ɸѡReapable_scenary����
    //��������һ����ЧĿ������֤ͨ��
    //״̬ͬ������
    //ͼ���л���green/transparent�����߼�״̬�ϸ��
    //���������������ڻ�����ʾ��ϵ
    //����״̬�����ͨ�����Է�װ������
}