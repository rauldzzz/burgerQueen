using UnityEngine;

public class Counter_Delivery : Counter
{
    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        OrderManager orderManager = FindObjectOfType<OrderManager>();
        if (orderManager == null) return;

        bool success = orderManager.CompleteOrder(player.heldItemName);

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