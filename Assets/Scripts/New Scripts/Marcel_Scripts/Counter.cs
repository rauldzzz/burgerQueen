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
    private CounterTimerUI currentTimerUI = null;

    public bool HasItem() => itemOnCounter != null;
    private bool waitingForPlayerToLeave = false;
    protected PlayerInteraction playerWhoPlacedItem = null;
    private PlayerInteraction lastPlayer = null;

    protected virtual bool WillInteract(PlayerInteraction player)
    {
        if (player == null) return false;

        bool playerHasItem = player.IsHoldingItem();
        bool counterHasItem = HasItem();

        // Can pick up item ONLY if not the player who placed it, or if they've left
        if (!playerHasItem && counterHasItem && player == playerWhoPlacedItem)
        {
            return false;
        }

        return (playerHasItem && !counterHasItem) || (!playerHasItem && counterHasItem);
    }

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
        if (playerInside != null) playerWhoPlacedItem = playerInside;
        Debug.Log($"Counter: Placed item {itemNameOnCounter} on {gameObject.name}.");
    }

    public virtual GameObject TakeItem()
    {
        if (itemOnCounter == null) return null;

        GameObject item = itemOnCounter;
        item.transform.SetParent(null);
        itemOnCounter = null;
        itemNameOnCounter = null;
        playerWhoPlacedItem = null;
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
                if (!foundPlayer.TryClaimCounter(this))
                {
                    return;
                }

                playerInside = foundPlayer;
                timer = 0f;
                currentTimerUI = playerInside.GetComponentInParent<CounterTimerUI>();
                if (WillInteract(playerInside) && currentTimerUI != null) currentTimerUI.Show();
                Debug.Log("Player detected by: " + gameObject.name);
            }

            if (playerInside == foundPlayer)
            {
                timer += Time.deltaTime;

                if (currentTimerUI != null) currentTimerUI.UpdateFill(timer, interactDelay);

                if (timer >= interactDelay && playerInside != null && WillInteract(playerInside))
                {
                    HandleInteraction(playerInside);
                    lastPlayer = playerInside;
                    playerInside = null;
                    timer = 0f;
                    waitingForPlayerToLeave = true;
                    if (currentTimerUI != null) currentTimerUI.Hide();
                }
            }
        }
        else
        {
            if (playerInside != null || waitingForPlayerToLeave)
            {
                Debug.Log("Player left: " + gameObject.name);
                if (currentTimerUI != null) currentTimerUI.Hide();
                if (playerInside != null && playerInside.sourceCounter == this)
                {
                    playerInside.sourceCounter = null;
                }
                if (playerInside != null)
                {
                    playerInside.ReleaseCounter(this);
                }
                if (lastPlayer != null && lastPlayer.sourceCounter == this)
                {
                    lastPlayer.sourceCounter = null;
                }
                if (lastPlayer != null)
                {
                    lastPlayer.ReleaseCounter(this);
                }
                playerInside = null;
                playerWhoPlacedItem = null;
                lastPlayer = null;
                waitingForPlayerToLeave = false;
                timer = 0f;
            }
        }
    }

    protected virtual void HandleInteraction(PlayerInteraction player)
    {
        if (player == null) return;

        if (player.IsHoldingItem() && !HasItem())
        {
            if (player.sourceCounter == this) return;

            GameObject dropped = player.Drop();
            PlaceItem(dropped, dropped.name);
            Debug.Log($"Counter: Player {player.name} placed item on {gameObject.name}.");
        }
        else if (!player.IsHoldingItem() && HasItem())
        {
            GameObject item = TakeItem();
            player.PickUp(item, item.name, this);
            Debug.Log($"Counter: Player {player.name} picked up item from {gameObject.name}.");
        }
    }
}