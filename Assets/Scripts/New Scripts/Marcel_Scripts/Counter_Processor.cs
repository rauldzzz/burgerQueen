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

    private bool isProcessing = false;
    private ProcessRecipe activeRecipe = null;

    protected override void Update()
    {
        if (playerInside == null || !isProcessing) return;

        timer += Time.deltaTime;

        // TODO: update progress circle here using (timer / interactDelay)

        if (timer >= interactDelay)
        {
            FinishProcessing();
            timer = 0f;
        }
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (isProcessing) return;

        if (player.IsHoldingItem() && !HasItem())
        {
            // Check if the held item matches a recipe
            ProcessRecipe match = recipes.Find(r => r.inputPrefab.name == player.heldItemName);
            if (match != null)
            {
                GameObject dropped = player.Drop();
                PlaceItem(dropped, dropped.name);
                activeRecipe = match;
                isProcessing = true;
                timer = 0f;
                Debug.Log("Processing: " + match.inputPrefab.name);
            }
            else
            {
                Debug.Log("This station can't process: " + player.heldItemName);
            }
        }
        else if (!player.IsHoldingItem() && HasItem() && !isProcessing)
        {
            // Pick up finished item
            GameObject item = TakeItem();
            player.PickUp(item, item.name);
        }
    }

    private void FinishProcessing()
    {
        if (activeRecipe == null) return;

        // Destroy the input item on the counter
        if (itemOnCounter != null)
            Destroy(itemOnCounter);

        // Spawn the output item on the counter
        GameObject output = Instantiate(activeRecipe.outputPrefab);
        output.transform.localScale = activeRecipe.outputScale;
        PlaceItem(output, activeRecipe.outputPrefab.name);

        isProcessing = false;
        activeRecipe = null;
        Debug.Log("Processing done. Item ready.");
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        // Leaving resets the timer but doesn't cancel processing
        // The station keeps going even if player walks away
    }
}