using UnityEngine;

public class Counter_Ingredient : Counter
{
    [Header("Ingredient Settings")]
    public GameObject ingredientPrefab;
    public Vector3 itemScale = Vector3.one;

    protected override bool WillInteract(PlayerInteraction player)
    {
        if (player == null) return false;
        return !player.IsHoldingItem();
    }

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (player.IsHoldingItem()) return;

        GameObject newItem = Instantiate(ingredientPrefab);
        newItem.transform.localScale = itemScale;
        player.PickUp(newItem, ingredientPrefab.name, this);
        Debug.Log("Picked up: " + ingredientPrefab.name);
    }
}