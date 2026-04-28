using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("Counter Settings")]
    public float interactDelay = 1f;

    protected GameObject itemOnCounter = null;
    protected string itemNameOnCounter = null;
    protected float timer = 0f;
    protected PlayerInteraction playerInside = null;
    private Collider counterCollider;

    public bool HasItem() => itemOnCounter != null;

    protected virtual void Start()
    {
        counterCollider = GetComponent<Collider>();
    }

    public virtual void PlaceItem(GameObject item, string itemName)
    {
        itemOnCounter = item;
        itemNameOnCounter = itemName;
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0);
    }

    public virtual GameObject TakeItem()
    {
        if (itemOnCounter == null) return null;

        GameObject item = itemOnCounter;
        item.transform.SetParent(null);
        itemOnCounter = null;
        itemNameOnCounter = null;
        return item;
    }

    protected virtual void Update()
    {
        if (counterCollider == null) return;

        Collider[] hits = Physics.OverlapBox(
            counterCollider.bounds.center,
            counterCollider.bounds.extents * 0.9f,
            transform.rotation
        );

        PlayerInteraction foundPlayer = null;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerInteraction p = hit.GetComponent<PlayerInteraction>();
            if (p != null)
            {
                foundPlayer = p;
                break;
            }
        }

        if (foundPlayer != null)
        {
            if (playerInside == null)
            {
                playerInside = foundPlayer;
                timer = 0f;
                Debug.Log("Player detected by: " + gameObject.name);
            }

            timer += Time.deltaTime;

            if (timer >= interactDelay)
            {
                HandleInteraction(playerInside);
                timer = 0f;
            }
        }
        else
        {
            if (playerInside != null)
            {
                Debug.Log("Player left: " + gameObject.name);
                playerInside = null;
                timer = 0f;
            }
        }
    }

    protected virtual void HandleInteraction(PlayerInteraction player)
    {
        if (player.IsHoldingItem() && !HasItem())
        {
            GameObject dropped = player.Drop();
            PlaceItem(dropped, dropped.name);
        }
        else if (!player.IsHoldingItem() && HasItem())
        {
            GameObject item = TakeItem();
            player.PickUp(item, item.name);
        }
    }
}