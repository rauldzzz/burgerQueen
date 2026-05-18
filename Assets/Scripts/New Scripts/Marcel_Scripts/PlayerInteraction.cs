using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject heldItem = null;
    public string heldItemName = null;

    public bool IsHoldingItem() => heldItem != null;

    public void PickUp(GameObject item, string itemName)
    {
        heldItem = item;
        heldItemName = Counter.NormalizeItemName(itemName);
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 13f, 3f);
    }

    public GameObject Drop()
    {
        if (heldItem == null) return null;

        GameObject dropped = heldItem;
        dropped.transform.SetParent(null);
        heldItem = null;
        heldItemName = null;
        return dropped;
    }
}