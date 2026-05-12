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

        Debug.Log($"[Counter_Hostess] Start on {name}. autoSpawnStartingState={autoSpawnStartingState}, defaultRecipe={(defaultRecipe != null ? defaultRecipe.burgerName : "null")}, recipes={recipes.Count}");

        if (ordersManager == null)
        {
            ordersManager = FindObjectOfType<OrdersManager>();
        }

        Debug.Log($"[Counter_Hostess] OrdersManager={(ordersManager != null ? ordersManager.name : "null")}, CurrentOrder={(ordersManager != null && ordersManager.CurrentOrder != null ? ordersManager.CurrentOrder.burgerName : "null")}");

        if (autoSpawnStartingState)
        {
            EnsureStartingState();
        }

        Debug.Log($"[Counter_Hostess] After start setup: HasItem={HasItem()}, itemOnCounter={(itemOnCounter != null ? itemOnCounter.name : "null")}, itemNameOnCounter={(itemNameOnCounter ?? "null")}");
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        Debug.Log($"[Counter_Hostess] Interaction on {name}. player={(player != null ? player.name : "null")}, playerHolding={(player != null && player.IsHoldingItem())}, heldItem={(player != null && player.heldItem != null ? player.heldItem.name : "null")}, heldItemName={(player != null ? player.heldItemName : "null")}, counterHasItem={HasItem()}, counterItem={(itemOnCounter != null ? itemOnCounter.name : "null")}, counterItemName={(itemNameOnCounter ?? "null")}");

        if (player == null)
        {
            Debug.LogWarning($"[Counter_Hostess] Null player on {name}.");
            return;
        }

        if (!player.IsHoldingItem())
        {
            Debug.Log($"[Counter_Hostess] Player is not holding anything, nothing to advance on {name}.");
            return;
        }

        if (!HasItem())
        {
            if (TryPlaceStartingState(player))
            {
                return;
            }

            Debug.LogWarning($"[Counter_Hostess] Counter {name} is empty and the held item '{player.heldItemName}' does not match the starting state for the active recipe.");
            return;
        }

        BurgerAssemblyRecipe.BurgerAssemblyStep step = FindMatchingStep(itemNameOnCounter, player.heldItemName);
        if (step == null)
        {
            Debug.LogWarning($"[Counter_Hostess] No matching step found on {name} for currentState='{itemNameOnCounter}' and heldItem='{player.heldItemName}'.");
            return;
        }

        Debug.Log($"[Counter_Hostess] Match found. currentState='{itemNameOnCounter}', heldItem='{player.heldItemName}', nextState='{step.nextStatePrefab.name}'");

        GameObject currentState = TakeItem();
        if (currentState != null)
        {
            Debug.Log($"[Counter_Hostess] Destroying current counter item '{currentState.name}'.");
            Destroy(currentState);
        }

        GameObject heldItem = player.Drop();
        if (heldItem != null)
        {
            Debug.Log($"[Counter_Hostess] Destroying player item '{heldItem.name}'.");
            Destroy(heldItem);
        }

        GameObject nextState = Instantiate(step.nextStatePrefab);
        PlaceItemAt(nextState, step.nextStatePrefab.name, itemSpawnPosition);

        Debug.Log("Burger advanced to: " + step.nextStatePrefab.name);
    }

    public void PlaceItemAt(GameObject item, string itemName, Vector3 localPosition)
    {
        itemOnCounter = item;
        itemNameOnCounter = itemName;
        item.transform.SetParent(transform);
        item.transform.localPosition = localPosition;
        item.transform.localScale = Vector3.one * itemScale;
    }

    private bool TryPlaceStartingState(PlayerInteraction player)
    {
        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null)
        {
            return false;
        }

        GameObject startingPrefab = recipe.startingStatePrefab;
        if (startingPrefab == null || startingPrefab.name != player.heldItemName)
        {
            return false;
        }

        GameObject heldItem = player.Drop();
        if (heldItem == null)
        {
            return false;
        }

        PlaceItemAt(heldItem, heldItem.name, itemSpawnPosition);
        Debug.Log($"[Counter_Hostess] Placed starting state '{heldItem.name}' on empty {name}.");
        return true;
    }

    private void EnsureStartingState()
    {
        if (HasItem()) return;

        BurgerAssemblyRecipe recipe = GetRecipeForCurrentOrder() ?? (defaultRecipe != null ? defaultRecipe : FindFirstRecipeWithStartingState());
        if (recipe == null) return;

        Debug.Log($"[Counter_Hostess] EnsureStartingState using recipe '{recipe.burgerName}'. startingState={(recipe.startingStatePrefab != null ? recipe.startingStatePrefab.name : "null")}");

        GameObject prefabToSpawn = recipe.startingStatePrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = FindFirstCurrentStatePrefab(recipe);
            Debug.Log($"[Counter_Hostess] startingStatePrefab was null, fallback spawn candidate={(prefabToSpawn != null ? prefabToSpawn.name : "null")}");
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[Counter_Hostess] No starting prefab could be resolved for recipe '{recipe.burgerName}'. The hostess will remain empty.");
            return;
        }

        GameObject startingState = Instantiate(prefabToSpawn);
        PlaceItemAt(startingState, prefabToSpawn.name, itemSpawnPosition);
        Debug.Log($"[Counter_Hostess] Spawned starting state '{prefabToSpawn.name}' on {name}.");
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
            Debug.Log($"[Counter_Hostess] Matched by current order recipe '{orderRecipe.burgerName}'.");
            return orderStep;
        }

        // 2) Try defaultRecipe assigned in inspector
        BurgerAssemblyRecipe recipe = defaultRecipe;
        if (recipe != null && recipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep step))
        {
            Debug.Log($"[Counter_Hostess] Matched by defaultRecipe '{recipe.burgerName}'.");
            return step;
        }

        // 3) Try other recipes list
        for (int i = 0; i < recipes.Count; i++)
        {
            recipe = recipes[i];
            if (recipe == null) continue;

            if (recipe.TryGetStep(currentStateName, heldItemName, out BurgerAssemblyRecipe.BurgerAssemblyStep matchingStep))
            {
                Debug.Log($"[Counter_Hostess] Matched by recipes list '{recipe.burgerName}'.");
                return matchingStep;
            }
        }

        Debug.LogWarning($"[Counter_Hostess] No recipe matched currentState='{currentStateName}' and heldItem='{heldItemName}'.");
        return null;
    }

    private BurgerAssemblyRecipe GetRecipeForCurrentOrder()
    {
        // Prefer explicit reference
        if (ordersManager == null)
        {
            ordersManager = FindObjectOfType<OrdersManager>();
        }

        if (ordersManager == null)
        {
            Debug.LogWarning($"[Counter_Hostess] No OrdersManager found in scene.");
            return null;
        }

        var current = ordersManager.CurrentOrder;
        if (current == null)
        {
            Debug.LogWarning($"[Counter_Hostess] OrdersManager '{ordersManager.name}' has no CurrentOrder yet.");
            return null;
        }

        // Match by burgerName
        string orderName = current.burgerName;
        Debug.Log($"[Counter_Hostess] Resolving recipe for CurrentOrder='{orderName}'.");
        if (defaultRecipe != null && defaultRecipe.burgerName == orderName) return defaultRecipe;

        for (int i = 0; i < recipes.Count; i++)
        {
            var r = recipes[i];
            if (r == null) continue;
            if (r.burgerName == orderName) return r;
        }

        Debug.LogWarning($"[Counter_Hostess] No BurgerAssemblyRecipe found with burgerName='{orderName}'. Check names against OrdersManager and the recipe assets.");
        return null;
    }
}