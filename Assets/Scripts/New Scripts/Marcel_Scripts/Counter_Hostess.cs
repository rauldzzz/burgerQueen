using UnityEngine;
using System.Collections.Generic;

public class Counter_Hostess : Counter
{
    [Header("Hostess Settings")]
    public BurgerAssemblyRecipe defaultRecipe;
    public List<BurgerAssemblyRecipe> recipes = new List<BurgerAssemblyRecipe>();
    public bool autoSpawnStartingState = true;

    protected override void Start()
    {
        base.Start();

        if (autoSpawnStartingState)
        {
            EnsureStartingState();
        }
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;
        if (!HasItem()) return;

        BurgerAssemblyRecipe.BurgerAssemblyStep step = FindMatchingStep(itemNameOnCounter, player.heldItemName);
        if (step == null) return;

        GameObject currentState = TakeItem();
        if (currentState != null)
        {
            Destroy(currentState);
        }

        GameObject heldItem = player.Drop();
        if (heldItem != null)
        {
            Destroy(heldItem);
        }

        GameObject nextState = Instantiate(step.nextStatePrefab);
        PlaceItem(nextState, step.nextStatePrefab.name);

        Debug.Log("Burger advanced to: " + step.nextStatePrefab.name);
    }

    private void EnsureStartingState()
    {
        if (HasItem()) return;

        BurgerAssemblyRecipe recipe = defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState();
        if (recipe == null) return;
        if (recipe.startingStatePrefab == null) return;

        GameObject startingState = Instantiate(recipe.startingStatePrefab);
        PlaceItem(startingState, recipe.startingStatePrefab.name);
    }

    private BurgerAssemblyRecipe FindFirstRecipeWithStartingState()
    {
        for (int i = 0; i < recipes.Count; i++)
        {
            BurgerAssemblyRecipe recipe = recipes[i];
            if (recipe == null) continue;
            if (recipe.startingStatePrefab == null) continue;

            return recipe;
        }

        return null;
    }

    private BurgerAssemblyRecipe.BurgerAssemblyStep FindMatchingStep(string currentStateName, string heldItemName)
    {
        BurgerAssemblyRecipe recipe = defaultRecipe;
        if (recipe != null && recipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep step))
        {
            return step;
        }

        for (int i = 0; i < recipes.Count; i++)
        {
            recipe = recipes[i];
            if (recipe == null) continue;

            if (recipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep matchingStep))
            {
                return matchingStep;
            }
        }

        return null;
    }
}