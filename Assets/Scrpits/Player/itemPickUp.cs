
using UnityEngine;

public class itemPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();

        if(item != null)
        {
            //Get item details
            ItemDetails itemDetails=InventoryManager.Instance.GetItemDetails(item.ItemCode);

            Debug.Log(itemDetails.itemDescription);
        }
    }
}
