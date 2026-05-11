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
        base.Update();

        if (isProcessing)
        {
            timer += Time.deltaTime;

            if (timer >= interactDelay)
            {
                FinishProcessing();
                timer = 0f;
            }
        }
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (isProcessing) return;
        if (!player.IsHoldingItem()) return;

        // Check if held item matches any recipe
        ProcessRecipe match = recipes.Find(r => r.inputPrefab.name == player.heldItemName);

        if (match != null)
        {
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

    private void FinishProcessing()
    {
        if (activeRecipe == null) return;
        if (playerInside == null) return;

        // Destroy the current held item
        Destroy(playerInside.heldItem);

        // Spawn the output and give it to the player
        GameObject output = Instantiate(activeRecipe.outputPrefab);
        output.transform.localScale = activeRecipe.outputScale;
        playerInside.PickUp(output, activeRecipe.outputPrefab.name);

        isProcessing = false;
        activeRecipe = null;
        Debug.Log("Processing done: " + activeRecipe?.outputPrefab.name);
    }
}