using UnityEngine;
using System.Collections.Generic;

public class Counter_Hostess : Counter
{
    [Header("Hostess Settings")]
    public BurgerAssemblyRecipe defaultRecipe;
    public List<BurgerAssemblyRecipe> recipes = new List<BurgerAssemblyRecipe>();
    public bool autoSpawnStartingState = true;
    public OrdersManager ordersManager;

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
        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
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
        // 1) Try to use the recipe currently selected by OrdersManager (preferred)
        BurgerAssemblyRecipe orderRecipe = GetRecipeForCurrentOrder();
        if (orderRecipe != null && orderRecipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep orderStep))
        {
            return orderStep;
        }

        // 2) Try defaultRecipe assigned in inspector
        BurgerAssemblyRecipe recipe = defaultRecipe;
        if (recipe != null && recipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep step))
        {
            return step;
        }

        // 3) Try other recipes list
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

    private BurgerAssemblyRecipe GetRecipeForCurrentOrder()
    {
        // Prefer explicit reference
        if (ordersManager == null)
        {
            ordersManager = FindObjectOfType<OrdersManager>();
        }

        if (ordersManager == null) return null;

        var current = ordersManager.CurrentOrder;
        if (current == null) return null;

        // Match by burgerName
        string orderName = current.burgerName;
        if (defaultRecipe != null && defaultRecipe.burgerName == orderName) return defaultRecipe;

        for (int i = 0; i < recipes.Count; i++)
        {
            var r = recipes[i];
            if (r == null) continue;
            if (r.burgerName == orderName) return r;
        }

        return null;
    }
}