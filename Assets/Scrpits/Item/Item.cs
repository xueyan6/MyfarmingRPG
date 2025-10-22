
using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField]
    private int _itemCode;

    private SpriteRenderer spriteRenderer;

    public int ItemCode {  get { return _itemCode; } set { _itemCode = value; } }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if(ItemCode!=0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCodeParam)//ItemCode=itemCodeParam当涉及到名称映射、键值查找、序列化/反序列化时，变量名必须一致；而在值传递、局部变量使用时，名称可以自由选择。
    {
        if (itemCodeParam != 0)
        {
            ItemCode = itemCodeParam;

            ItemDetails itemDetails=InventoryManager.Instance.GetItemDetails(ItemCode);

            spriteRenderer.sprite = itemDetails.itemSprite;

            //if item type is reapable then add nudgeable component如果项目类型为可收割，则添加可推动组件
            if (itemDetails.itemType==ItemType.Reapable_scenary)
            {
                gameObject.AddComponent<ItemNudge>();
            }
        }
    }
}
