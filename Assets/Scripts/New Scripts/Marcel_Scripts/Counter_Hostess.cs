using UnityEngine;
using System.Collections.Generic;

public class Counter_Hostess : Counter
{
    [Header("Hostess Settings")]
    public BurgerAssemblyRecipe defaultRecipe;
    public List<BurgerAssemblyRecipe> recipes = new List<BurgerAssemblyRecipe>();
    public bool autoSpawnStartingState = false;
    public OrdersManager ordersManager;
    public Vector3 itemSpawnPosition = new Vector3(0, 1f, 0);
    public float itemScale = 4f;

    protected override void Start()
    {
        base.Start();

        if (ordersManager == null)
        {
            ordersManager = FindFirstObjectByType<OrdersManager>();
        }

        if (ordersManager != null)
        {
            ordersManager.CurrentOrderChanged += HandleCurrentOrderChanged;
        }

        SyncToCurrentOrder();
    }

    private void OnDestroy()
    {
        if (ordersManager != null)
        {
            ordersManager.CurrentOrderChanged -= HandleCurrentOrderChanged;
        }
    }

    protected override bool WillInteract(PlayerInteraction player)
    {
        if (player == null) return false;

        if (!player.IsHoldingItem() && HasItem())
        {
            if (IsFinalBurgerStateForAnyRecipe(itemNameOnCounter))
            {
                return true;
            }

            BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? defaultRecipe;
            return recipe != null && CanStepBack(recipe, itemNameOnCounter);
        }

        if (player.IsHoldingItem() && !HasItem())
        {
            return TryFindStartingRecipe(player.heldItemName) != null;
        }

        if (player.IsHoldingItem() && HasItem())
        {
            return FindMatchingStep(itemNameOnCounter, player.heldItemName) != null;
        }

        return false;
    }

    private BurgerAssemblyRecipe TryFindStartingRecipe(string heldItemName)
    {
        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null) return null;

        GameObject startingPrefab = recipe.startingStatePrefab;
        if (startingPrefab != null && Counter.NormalizeItemName(startingPrefab.name) == Counter.NormalizeItemName(heldItemName))
        {
            return recipe;
        }

        return null;
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (player == null) return;

        if (!player.IsHoldingItem())
        {
            if (HasItem())
            {
                if (IsFinalBurgerStateForAnyRecipe(itemNameOnCounter))
                {
                    GameObject finalBurger = TakeItem();
                    if (finalBurger != null)
                    {
                        player.PickUp(finalBurger, finalBurger.name, this);
                    }

                    return;
                }

                BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? defaultRecipe;
                if (recipe == null)
                {
                    return;
                }

                if (TryStepBackAndTakeIngredient(recipe, player))
                {
                    return;
                }

                if (!IsFinalBurgerState(recipe, itemNameOnCounter))
                {
                    return;
                }

                GameObject item = TakeItem();
                if (item != null)
                {
                    player.PickUp(item, item.name, this);
                }
            }

            return;
        }

        if (!HasItem())
        {
            if (TryPlaceStartingState(player))
            {
                return;
            }

            return;
        }

        BurgerAssemblyRecipe.BurgerAssemblyStep step = FindMatchingStep(itemNameOnCounter, player.heldItemName);
        if (step == null)
        {
            return;
        }

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
        PlaceItemAt(nextState, step.nextStatePrefab.name, itemSpawnPosition);

        Debug.Log("Burger advanced to: " + step.nextStatePrefab.name);
    }

    private bool TryStepBackAndTakeIngredient(BurgerAssemblyRecipe recipe, PlayerInteraction player)
    {
        if (recipe == null || player == null || string.IsNullOrEmpty(itemNameOnCounter))
        {
            return false;
        }

        if (!recipe.TryGetStepByNextState(itemNameOnCounter, out BurgerAssemblyRecipe.BurgerAssemblyStep step))
        {
            return false;
        }

        if (step.currentStatePrefab == null || step.requiredIngredientPrefab == null)
        {
            return false;
        }

        GameObject currentState = TakeItem();
        if (currentState != null)
        {
            Destroy(currentState);
        }

        GameObject previousState = Instantiate(step.currentStatePrefab);
        PlaceItemAt(previousState, step.currentStatePrefab.name, itemSpawnPosition);

        GameObject ingredient = Instantiate(step.requiredIngredientPrefab);
        ingredient.transform.localScale = Vector3.one * itemScale;
        player.PickUp(ingredient, ingredient.name, this);

        Debug.Log("Burger stepped back to: " + step.currentStatePrefab.name + " and ingredient returned: " + step.requiredIngredientPrefab.name);
        return true;
    }

    public void PlaceItemAt(GameObject item, string itemName, Vector3 localPosition)
    {
        itemOnCounter = item;
        itemNameOnCounter = itemName;
        item.transform.SetParent(transform);
        item.transform.localPosition = localPosition;
        item.transform.localScale = Vector3.one * itemScale;
        if (playerInside != null) playerWhoPlacedItem = playerInside;
    }

    private bool TryPlaceStartingState(PlayerInteraction player)
    {
        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null)
        {
            return false;
        }

        GameObject startingPrefab = recipe.startingStatePrefab;
        if (startingPrefab == null || Counter.NormalizeItemName(startingPrefab.name) != Counter.NormalizeItemName(player.heldItemName))
        {
            return false;
        }

        GameObject heldItem = player.Drop();
        if (heldItem == null)
        {
            return false;
        }

        PlaceItemAt(heldItem, heldItem.name, itemSpawnPosition);
        return true;
    }

    private void EnsureStartingState()
    {
        if (HasItem()) return;

        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null) return;

        GameObject prefabToSpawn = recipe.startingStatePrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = FindFirstCurrentStatePrefab(recipe);
        }

        if (prefabToSpawn == null)
        {
            return;
        }

        GameObject startingState = Instantiate(prefabToSpawn);
        PlaceItemAt(startingState, prefabToSpawn.name, itemSpawnPosition);
    }

    private void SyncToCurrentOrder()
    {
        if (ordersManager == null)
            return;

        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null)
            return;

        if (autoSpawnStartingState)
        {
            EnsureStartingState();
        }
    }

    private void HandleCurrentOrderChanged()
    {
        SyncToCurrentOrder();
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

    private GameObject FindFirstCurrentStatePrefab(BurgerAssemblyRecipe recipe)
    {
        if (recipe == null) return null;

        for (int i = 0; i < recipe.steps.Count; i++)
        {
            BurgerAssemblyRecipe.BurgerAssemblyStep step = recipe.steps[i];
            if (step == null) continue;
            if (step.currentStatePrefab != null) return step.currentStatePrefab;
            if (step.nextStatePrefab != null) return step.nextStatePrefab;
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

    private bool IsFinalBurgerState(BurgerAssemblyRecipe recipe, string stateName)
    {
        if (recipe == null || string.IsNullOrEmpty(stateName))
        {
            return false;
        }

        string normalizedStateName = Counter.NormalizeItemName(stateName);
        bool matchesAnyNextState = false;

        for (int i = 0; i < recipe.steps.Count; i++)
        {
            BurgerAssemblyRecipe.BurgerAssemblyStep step = recipe.steps[i];
            if (step == null || step.nextStatePrefab == null)
            {
                continue;
            }

            if (Counter.NormalizeItemName(step.nextStatePrefab.name) == normalizedStateName)
            {
                matchesAnyNextState = true;
            }

            if (step.currentStatePrefab != null && Counter.NormalizeItemName(step.currentStatePrefab.name) == normalizedStateName)
            {
                return false;
            }
        }

        return matchesAnyNextState;
    }

    private bool CanStepBack(BurgerAssemblyRecipe recipe, string stateName)
    {
        if (recipe == null || string.IsNullOrEmpty(stateName))
        {
            return false;
        }

        return recipe.TryGetStepByNextState(stateName, out _);
    }

    private bool IsFinalBurgerStateForAnyRecipe(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
        {
            return false;
        }

        if (defaultRecipe != null && IsFinalBurgerState(defaultRecipe, stateName))
        {
            return true;
        }

        for (int i = 0; i < recipes.Count; i++)
        {
            BurgerAssemblyRecipe recipe = recipes[i];
            if (recipe == null) continue;

            if (IsFinalBurgerState(recipe, stateName))
            {
                return true;
            }
        }

        return false;
    }

    private BurgerAssemblyRecipe GetRecipeForCurrentOrder()
    {
        // Prefer explicit reference
        if (ordersManager == null)
        {
            ordersManager = FindFirstObjectByType<OrdersManager>();
        }

        if (ordersManager == null)
        {
            return null;
        }

        var current = ordersManager.CurrentOrder;
        if (current == null)
        {
            return null;
        }

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