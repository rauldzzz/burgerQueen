using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("Counter Settings")]
    public float interactDelay = 1f;

    protected GameObject itemOnCounter = null;
    protected string itemNameOnCounter = null;

    protected float timer = 0f;
    protected PlayerInteraction playerInside = null;

    public bool HasItem() => itemOnCounter != null;

    // Places an item visually on top of the counter
    public virtual void PlaceItem(GameObject item, string itemName)
    {
        itemOnCounter = item;
        itemNameOnCounter = itemName;
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0);
    }

    // Removes item from counter and returns it
    public virtual GameObject TakeItem()
    {
        if (itemOnCounter == null) return null;

        GameObject item = itemOnCounter;
        item.transform.SetParent(null);
        itemOnCounter = null;
        itemNameOnCounter = null;
        return item;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            Debug.Log("Player detected!");
            playerInside = player;
            timer = 0f;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerInteraction player = other.GetComponent<PlayerInteraction>();
        if (player != null)
        {
            playerInside = null;
            timer = 0f;
        }
    }

    protected virtual void Update()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position,
            GetComponent<Collider>().bounds.extents * 0.9f // slightly smaller than the collider
        );

        PlayerInteraction foundPlayer = null;

        foreach (Collider hit in hits)
        {
            // Only detect objects tagged as Player
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
                Debug.Log("Player detected!");
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
                Debug.Log("Player left.");
                playerInside = null;
                timer = 0f;
            }
        }
    }

    // Override in subclasses for custom behaviour
    protected virtual void HandleInteraction(PlayerInteraction player)
    {
        if (player.IsHoldingItem() && !HasItem())
        {
            // Player drops item onto counter
            GameObject dropped = player.Drop();
            PlaceItem(dropped, dropped.name);
        }
        else if (!player.IsHoldingItem() && HasItem())
        {
            // Player picks up item from counter
            GameObject item = TakeItem();
            player.PickUp(item, item.name);
        }
    }
}