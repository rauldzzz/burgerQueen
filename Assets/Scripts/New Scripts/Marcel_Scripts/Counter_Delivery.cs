using UnityEngine;

public class Counter_Delivery : Counter
{
    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        OrdersManager orderManager = FindObjectOfType<OrdersManager>();
        if (orderManager == null) return;

        bool success = orderManager.CompleteOrder(Counter.NormalizeItemName(player.heldItemName));

        if (success)
        {
            Debug.Log("Order delivered successfully!");
            Destroy(player.heldItem);
            player.Drop();
        }
        else
        {
            Debug.Log("Wrong order! Dish discarded.");
            Destroy(player.heldItem);
            player.Drop();
        }
    }
}