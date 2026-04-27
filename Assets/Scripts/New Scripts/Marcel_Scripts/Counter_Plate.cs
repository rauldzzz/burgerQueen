using UnityEngine;
using System.Collections.Generic;

public class Counter_Plate : Counter
{
    [System.Serializable]
    public class PlateRecipe
    {
        public string recipeName;
        public List<string> requiredIngredients;
        public GameObject completedDishPrefab;
        public Vector3 outputScale = Vector3.one;
    }

    [Header("Plate Settings")]
    public List<PlateRecipe> recipes;

    private bool hasPlate = false;
    private List<string> ingredientsOnPlate = new List<string>();

    protected override void HandleInteraction(PlayerInteraction player)
    {
        if (!hasPlate)
        {
            // No plate yet — only accept a plate
            if (player.IsHoldingItem() && player.heldItemName == "Plate")
            {
                GameObject dropped = player.Drop();
                PlaceItem(dropped, "Plate");
                hasPlate = true;
                Debug.Log("Plate placed on counter.");
            }
            else if (!player.IsHoldingItem() && HasItem())
            {
                // Pick up whatever is on the counter (e.g. completed dish)
                GameObject item = TakeItem();
                player.PickUp(item, item.name);
                hasPlate = false;
                ingredientsOnPlate.Clear();
            }
        }
        else
        {
            // Plate is here — accept ingredients
            if (player.IsHoldingItem() && player.heldItemName != "Plate")
            {
                string ingredient = player.heldItemName;
                GameObject dropped = player.Drop();
                Destroy(dropped); // Ingredient merges into plate visually
                ingredientsOnPlate.Add(ingredient);
                Debug.Log("Added to plate: " + ingredient);

                // Check if any recipe is complete
                CheckRecipes();
            }
            else if (!player.IsHoldingItem() && HasItem())
            {
                // Pick up completed dish or plate
                GameObject item = TakeItem();
                player.PickUp(item, item.name);
                hasPlate = false;
                ingredientsOnPlate.Clear();
            }
        }
    }

    private void CheckRecipes()
    {
        foreach (PlateRecipe recipe in recipes)
        {
            if (RecipeMatches(recipe))
            {
                CompleteDish(recipe);
                return;
            }
        }
    }

    private bool RecipeMatches(PlateRecipe recipe)
    {
        if (ingredientsOnPlate.Count != recipe.requiredIngredients.Count) return false;

        List<string> copy = new List<string>(ingredientsOnPlate);
        foreach (string required in recipe.requiredIngredients)
        {
            if (!copy.Remove(required)) return false;
        }
        return true;
    }

    private void CompleteDish(PlateRecipe recipe)
    {
        // Destroy the plate on the counter
        if (itemOnCounter != null)
            Destroy(itemOnCounter);

        // Spawn the completed dish
        GameObject dish = Instantiate(recipe.completedDishPrefab);
        dish.transform.localScale = recipe.outputScale;
        PlaceItem(dish, recipe.completedDishPrefab.name);

        ingredientsOnPlate.Clear();
        hasPlate = false;
        Debug.Log("Recipe complete: " + recipe.recipeName);
    }
}