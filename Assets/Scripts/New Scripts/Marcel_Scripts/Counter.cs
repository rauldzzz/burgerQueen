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
    protected float originalInteractDelay = 1f;

    public bool HasItem() => itemOnCounter != null;
    private bool waitingForPlayerToLeave = false;
    public static string NormalizeItemName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return name.Replace("(Clone)", "").Trim();
    }

    protected virtual void Awake()
    {
        originalInteractDelay = interactDelay;
        Debug.Log($"Counter: Awake on {gameObject.name}, originalInteractDelay={originalInteractDelay}.");
    }

    protected virtual void Start()
    {
        counterCollider = GetComponent<Collider>();
    }

    public void ApplyInteractDelayMultiplier(float multiplier)
    {
        interactDelay = originalInteractDelay * multiplier;
        Debug.Log($"Counter: ApplyInteractDelayMultiplier on {gameObject.name} -> interactDelay={interactDelay} (mult={multiplier}).");
    }

    public void ResetInteractDelay()
    {
        interactDelay = originalInteractDelay;
        Debug.Log($"Counter: ResetInteractDelay on {gameObject.name} -> interactDelay={interactDelay}.");
    }

    public virtual void PlaceItem(GameObject item, string itemName)
    {
        itemOnCounter = item;
        itemNameOnCounter = NormalizeItemName(itemName);
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 1f, 0);
        Debug.Log($"Counter: Placed item {itemNameOnCounter} on {gameObject.name}.");
    }

    public virtual GameObject TakeItem()
    {
        if (itemOnCounter == null) return null;

        GameObject item = itemOnCounter;
        item.transform.SetParent(null);
        itemOnCounter = null;
        itemNameOnCounter = null;
        Debug.Log($"Counter: Item taken from {gameObject.name}.");
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
            if (playerInside == null && !waitingForPlayerToLeave)
            {
                playerInside = foundPlayer;
                timer = 0f;
                Debug.Log("Player detected by: " + gameObject.name);
            }

            timer += Time.deltaTime;

            if (timer >= interactDelay && playerInside != null)
            {
                HandleInteraction(playerInside);
                playerInside = null;
                timer = 0f;
                waitingForPlayerToLeave = true; // block re-detection
            }
        }
        else
        {
            if (playerInside != null || waitingForPlayerToLeave)
            {
                Debug.Log("Player left: " + gameObject.name);
                playerInside = null;
                timer = 0f;
                waitingForPlayerToLeave = false; // player left, allow again
            }
        }
    }

    protected virtual void HandleInteraction(PlayerInteraction player)
    {
        if (player.IsHoldingItem() && !HasItem())
        {
            GameObject dropped = player.Drop();
            PlaceItem(dropped, dropped.name);
            Debug.Log($"Counter: Player {player.name} placed item on {gameObject.name}.");
        }
        else if (!player.IsHoldingItem() && HasItem())
        {
            GameObject item = TakeItem();
            player.PickUp(item, item.name);
            Debug.Log($"Counter: Player {player.name} picked up item from {gameObject.name}.");
        }
    }
}