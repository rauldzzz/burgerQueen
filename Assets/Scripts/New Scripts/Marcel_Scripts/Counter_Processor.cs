using UnityEngine;
using System.Collections.Generic;

public class Counter_Processor : Counter
{
    [System.Serializable]
    public class ProcessRecipe
    {
        public GameObject inputPrefab;
        public GameObject outputPrefab;
        public Vector3 outputScale = Vector3.one;
    }

    [Header("Processor Settings")]
    public List<ProcessRecipe> recipes;

    protected override void Start()
    {
        base.Start();
    }

    protected override bool WillInteract(PlayerInteraction player)
    {
        if (player == null) return false;
        if (!player.IsHoldingItem()) return false;
        return recipes.Find(r => Counter.NormalizeItemName(r.inputPrefab.name) == Counter.NormalizeItemName(player.heldItemName)) != null;
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        ProcessRecipe match = recipes.Find(r => Counter.NormalizeItemName(r.inputPrefab.name) == Counter.NormalizeItemName(player.heldItemName));

        if (match != null)
        {
            GameObject inputItem = player.Drop();
            if (inputItem != null)
            {
                Destroy(inputItem);
            }

            GameObject output = Instantiate(match.outputPrefab);
            output.transform.localScale = match.outputScale;
            player.PickUp(output, match.outputPrefab.name, this);
            Debug.Log($"Counter_Processor: Processed {match.inputPrefab.name} into {match.outputPrefab.name} for player {player.name} on {gameObject.name}.");
        }
        else
        {
            Debug.Log("This station can't process: " + player.heldItemName);
        }
    }
}