using UnityEngine;

public class Counter_Delivery : Counter
{
    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        OrdersManager orderManager = FindFirstObjectByType<OrdersManager>();
        if (orderManager == null) return;

        bool success = orderManager.CompleteOrder(Counter.NormalizeItemName(player.heldItemName));

        if (success)
        {
            Debug.Log("Order delivered successfully!");
            if (SoundManager.Instance != null)
            {
                Debug.Log("Counter_Delivery: Playing deliver SFX.");
                SoundManager.Instance.PlayDeliverClip();
            }
            Destroy(player.heldItem);
            player.Drop(false);
        }
        else
        {
            Debug.Log("Wrong order! Dish discarded.");
            Destroy(player.heldItem);
            player.Drop(false);
        }
    }
}