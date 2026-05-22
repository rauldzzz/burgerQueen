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
    private PlayerInteraction processingPlayer = null;
    private CounterTimerUI processingTimerUI = null;

    protected override bool WillInteract(PlayerInteraction player)
    {
        if (player == null || isProcessing) return false;
        if (!player.IsHoldingItem()) return false;
        return recipes.Find(r => Counter.NormalizeItemName(r.inputPrefab.name) == Counter.NormalizeItemName(player.heldItemName)) != null;
    }

    protected override void Update()
    {
        base.Update();

        if (isProcessing)
        {
            timer += Time.deltaTime;
            if (processingTimerUI != null) processingTimerUI.UpdateFill(timer, interactDelay);

            if (timer >= interactDelay)
            {
                FinishProcessing();
                timer = 0f;
                if (processingTimerUI != null) processingTimerUI.Hide();
            }
        }
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (isProcessing) return;
        if (!player.IsHoldingItem()) return;

        ProcessRecipe match = recipes.Find(r => Counter.NormalizeItemName(r.inputPrefab.name) == Counter.NormalizeItemName(player.heldItemName));

        if (match != null)
        {
            activeRecipe = match;
            processingPlayer = player;
            processingTimerUI = player.GetComponentInParent<CounterTimerUI>();
            isProcessing = true;
            timer = 0f;
            if (processingTimerUI != null) processingTimerUI.Show();
            Debug.Log($"Counter_Processor: Starting processing {match.inputPrefab.name} for player {player.name} on {gameObject.name}.");
        }
        else
        {
            Debug.Log("This station can't process: " + player.heldItemName);
        }
    }

    private void FinishProcessing()
    {
        if (activeRecipe == null) return;
        if (processingPlayer == null) return;

        if (processingPlayer.heldItem != null)
        {
            Destroy(processingPlayer.heldItem);
        }
        processingPlayer.heldItem = null;
        processingPlayer.heldItemName = null;

        GameObject output = Instantiate(activeRecipe.outputPrefab);
        output.transform.localScale = activeRecipe.outputScale;
        processingPlayer.PickUp(output, activeRecipe.outputPrefab.name, this);

        isProcessing = false;
        activeRecipe = null;
        processingPlayer = null;
        Debug.Log($"Counter_Processor: Finished processing on {gameObject.name}.");
    }
}