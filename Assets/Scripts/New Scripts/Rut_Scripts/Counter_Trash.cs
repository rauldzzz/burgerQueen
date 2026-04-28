using UnityEngine;

public class Counter_Trash : Counter
{
    protected override void HandleInteraction(PlayerInteraction player)
    {
        // Si no té res, no fem res
        if (!player.IsHoldingItem()) return;

        // Destruïm l'objecte que porta
        if (player.heldItem != null)
        {
            Destroy(player.heldItem);
        }

        // Buidem la mà del player
        player.Drop();

        Debug.Log("Item discarded.");
    }
}
