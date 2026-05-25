using UnityEngine;

public class WallWindowOpen : MonoBehaviour
{
    private OrdersManager ordersManager;
    private Collider windowCollider;
    private float checkInterval = 0.12f;
    private float checkTimer = 0f;

    private void Start()
    {
        ordersManager = FindFirstObjectByType<OrdersManager>();
        windowCollider = GetComponent<Collider>();

        if (windowCollider == null)
        {
            Debug.LogWarning("WallWindowOpen: no Collider found on this GameObject. Overlap checks will be skipped.");
        }
    }

    private void HandleCollision(GameObject otherObj)
    {
        if (otherObj == null) return;

        if (ordersManager == null)
            ordersManager = FindFirstObjectByType<OrdersManager>();

        if (ordersManager == null) return;

        var current = ordersManager.CurrentOrder;
        if (current == null || current.burger == null)
        {
            Debug.Log("WallWindowOpen: no current order to compare with. current=" + (current == null ? "null" : current.burgerName));
            return;
        }

        string deliveredName = Counter.NormalizeItemName(otherObj.name);
        string targetName = Counter.NormalizeItemName(current.burger.name);

        if (deliveredName == targetName)
        {
            Debug.Log("WallWindowOpen: Burger matches! Completing order '" + current.burgerName + "'.");
            ordersManager.CompleteOrder(current.burgerName);

            if (SoundManager.Instance != null)
            {
                Debug.Log("WallWindowOpen: Playing deliver SFX.");
                SoundManager.Instance.PlayDeliverClip();
            }

            Destroy(otherObj);  // Destroy the delivered burger
            Debug.Log("WallWindowOpen: Order completed. Burger destroyed. New order ready.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("WallWindowOpen: OnTriggerEnter with " + other.gameObject.name);
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("WallWindowOpen: OnCollisionEnter with " + collision.gameObject.name);
        HandleCollision(collision.gameObject);
    }

    private void Update()
    {
        if (windowCollider == null) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f) return;
        checkTimer = checkInterval;

        if (ordersManager == null)
            ordersManager = FindFirstObjectByType<OrdersManager>();

        if (ordersManager == null)
        {
            Debug.Log("WallWindowOpen: OrdersManager not found in Update().");
            return;
        }

        var current = ordersManager.CurrentOrder;
        if (current == null || current.burger == null)
        {
            Debug.Log("WallWindowOpen: No active order in Update().");
            return;
        }

        // Use OverlapSphere with a large radius to detect player holding the burger
        Vector3 searchCenter = windowCollider.bounds.center;
        float searchRadius = 15f; // Large radius to ensure player detection
        
        Collider[] hits = Physics.OverlapSphere(searchCenter, searchRadius, ~0, QueryTriggerInteraction.Collide);
        
        foreach (Collider c in hits)
        {
            // First try: check if this collider belongs to a PlayerInteraction holding the correct item
            GameObject candidate = CheckPlayerInteractionHeldItem(c, Counter.NormalizeItemName(current.burger.name));
            if (candidate != null)
            {
                Debug.Log("WallWindowOpen: Burger delivered! Completing order '" + current.burgerName + "'.");
                
                // Get the player and drop the item
                PlayerInteraction player = c.GetComponentInParent<PlayerInteraction>();
                if (player != null)
                {
                    HandleCollision(candidate);
                    player.Drop(false);  // Drop automatically after delivery (silent)
                }
                break;
            }

            // Fallback: try to find by name traversal (for non-player objects)
            candidate = FindMatchingRoot(c.transform, Counter.NormalizeItemName(current.burger.name));
            if (candidate != null)
            {
                Debug.Log("WallWindowOpen: Burger delivered! Completing order '" + current.burgerName + "'.");
                HandleCollision(candidate);
                break;
            }
        }
    }

    private GameObject FindMatchingRoot(Transform t, string targetNormalizedName)
    {
        if (t == null) return null;
        Transform cur = t;
        while (cur != null)
        {
            if (Counter.NormalizeItemName(cur.name) == targetNormalizedName)
                return cur.gameObject;
            cur = cur.parent;
        }

        // Also check attached rigidbody root (in case collider is on a child with a different parent chain)
        Rigidbody rb = t.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            if (Counter.NormalizeItemName(rb.gameObject.name) == targetNormalizedName)
                return rb.gameObject;
        }

        return null;
    }

    private GameObject CheckPlayerInteractionHeldItem(Collider c, string targetNormalizedName)
    {
        PlayerInteraction player = c.GetComponentInParent<PlayerInteraction>();
        if (player == null) return null;

        if (!player.IsHoldingItem()) return null;

        string heldNormalized = Counter.NormalizeItemName(player.heldItemName);

        if (heldNormalized == targetNormalizedName)
        {
            return player.heldItem;
        }

        return null;
    }
}
