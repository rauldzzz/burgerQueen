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
        // Run base detection so playerInside gets updated
        base.Update();

        // Keep processing even if player walks away
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

        if (player.IsHoldingItem() && !HasItem())
        {
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
            GameObject item = TakeItem();
            player.PickUp(item, item.name);
        }
    }

    private void FinishProcessing()
    {
        if (activeRecipe == null) return;

        if (itemOnCounter != null)
            Destroy(itemOnCounter);

        GameObject output = Instantiate(activeRecipe.outputPrefab);
        output.transform.localScale = activeRecipe.outputScale;
        PlaceItem(output, activeRecipe.outputPrefab.name);

        isProcessing = false;
        activeRecipe = null;
        Debug.Log("Processing done. Item ready.");
    }
}