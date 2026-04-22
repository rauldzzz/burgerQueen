using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    public List<string> heldIngredients = new List<string>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    void Interact()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();

            if (ingredient != null)
            {
                heldIngredients.Add(ingredient.ingredientName);
                Destroy(hit.collider.gameObject);

                Debug.Log("Has agafat: " + ingredient.ingredientName);
            }
        }
    }

    void Serve()
    {
        if (heldIngredients.Contains("Bread") && heldIngredients.Contains("Meat"))
        {
            bool success = FindObjectOfType<OrderManager>().CompleteOrder("Burger");

            if (success)
            {
                heldIngredients.Clear();
            }
        }
    }
}