using UnityEngine;

public class Counter_Trash : Counter
{
    protected override bool WillInteract(PlayerInteraction player)
    {
        if (player == null) return false;
        return player.IsHoldingItem();
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        // Si no t� res, no fem res
        if (!player.IsHoldingItem()) return;

        // Destru�m l'objecte que porta
        if (player.heldItem != null)
        {
            Destroy(player.heldItem);
        }

        // Buidem la m� del player
        player.Drop();

        Debug.Log("Item discarded.");
    }
}
