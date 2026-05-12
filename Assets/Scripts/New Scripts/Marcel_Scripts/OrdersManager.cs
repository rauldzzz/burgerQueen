using UnityEngine;
using UnityEngine.UI; // ¡Importante añadir esto para usar Image!
using System.Collections.Generic;
using TMPro;

public class OrdersManager : MonoBehaviour
{
    [Header("Orders Data")]
    public List<BurgerRecipe> possibleOrders;

    [Header("UI References")]
    public TextMeshProUGUI orderText;
    public Image finalBurgerDisplay; // Donde se mostrará la hamburguesa completa
    public Transform ingredientsContainer; // El contenedor donde aparecerán los iconos
    public GameObject ingredientIconPrefab; // Un prefab que será solo un objeto de UI Image

    private BurgerRecipe currentOrder;

    void Start()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        if (possibleOrders.Count == 0) return;

        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];

        UpdateUI();
        Debug.Log("New order: " + currentOrder.burgerName);
    }

    public bool CompleteOrder(string submittedBurgerName)
    {
        if (currentOrder == null) return false;

        if (submittedBurgerName == currentOrder.burgerName)
        {
            FindObjectOfType<ScoreManager>().AddMoney(currentOrder.reward);
            GenerateNewOrder();
            return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        // 1. Actualizamos el texto
        if (orderText != null)
            orderText.text = currentOrder.burgerName;

        // 2. Actualizamos la imagen principal de la hamburguesa
        if (finalBurgerDisplay != null && currentOrder.finalBurgerImage != null)
        {
            finalBurgerDisplay.sprite = currentOrder.finalBurgerImage;
        }

        // 3. Mostrar los ingredientes dinámicamente
        if (ingredientsContainer != null && ingredientIconPrefab != null)
        {
            // Primero, borramos los iconos de la orden anterior
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            // Luego, creamos un nuevo icono por cada ingrediente en la lista
            foreach (Sprite ingredientSprite in currentOrder.ingredientImages)
            {
                // Instanciamos el prefab dentro del contenedor
                GameObject newIcon = Instantiate(ingredientIconPrefab, ingredientsContainer);

                // Le asignamos el sprite correspondiente
                newIcon.GetComponent<Image>().sprite = ingredientSprite;
            }
        }
    }
}