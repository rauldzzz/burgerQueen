using UnityEngine;
using System.Collections.Generic;

public class Countertop_Grill : MonoBehaviour
{
    [Header("Grill Settings")]
    public float grillDelay = 3f;

    [System.Serializable]
    public class GrillRecipe
    {
        public GameObject inputPrefab;
        public GameObject outputPrefab;
    }

    public List<GrillRecipe> recipes;

    private float timer = 0f;
    private PlayerInteract playerInside = null;
    private GrillRecipe matchedRecipe = null;

    void Update()
    {
        if (playerInside == null || matchedRecipe == null) return;

        timer += Time.deltaTime;

        if (timer >= grillDelay)
        {
            playerInside.heldIngredients.Remove(matchedRecipe.inputPrefab.name);
            playerInside.heldIngredients.Add(matchedRecipe.outputPrefab.name);
            Debug.Log(matchedRecipe.inputPrefab.name + " ? " + matchedRecipe.outputPrefab.name);

            timer = 0f;
            matchedRecipe = null;
            playerInside = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player == null) return;

        foreach (GrillRecipe recipe in recipes)
        {
            if (player.heldIngredients.Contains(recipe.inputPrefab.name))
            {
                playerInside = player;
                matchedRecipe = recipe;
                timer = 0f;
                Debug.Log("Grilling: " + recipe.inputPrefab.name);
                return;
            }
        }

        Debug.Log("No grillable ingredient held.");
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteract player = other.GetComponent<PlayerInteract>();
        if (player != null)
        {
            playerInside = null;
            matchedRecipe = null;
            timer = 0f;
        }
    }
}