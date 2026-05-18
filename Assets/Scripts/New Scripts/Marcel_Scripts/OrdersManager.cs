using UnityEngine;
using UnityEngine.UI; // �Importante a�adir esto para usar Image!
using System.Collections.Generic;
using TMPro;

public class OrdersManager : MonoBehaviour
{
    [Header("Orders Data")]
    public List<BurgerRecipe> possibleOrders;

    [Header("UI References")]
    public TextMeshProUGUI orderText;
    public Image finalBurgerDisplay; // Donde se mostrar� la hamburguesa completa
    public Transform ingredientsContainer; // El contenedor donde aparecer�n los iconos
    public GameObject ingredientIconPrefab; // Un prefab que ser� solo un objeto de UI Image

    private BurgerRecipe currentOrder;

    public BurgerRecipe CurrentOrder => currentOrder;

    void Start()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        if (possibleOrders.Count == 0) return;

        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];

        UpdateUI();
        Debug.Log("OrdersManager: New order generated: " + currentOrder.burgerName + " (reward=" + currentOrder.reward + ")");
    }

    public bool CompleteOrder(string submittedBurgerName)
    {
        if (currentOrder == null)
        {
            Debug.LogWarning("OrdersManager: CompleteOrder called but no currentOrder is set.");
            return false;
        }

        if (submittedBurgerName == currentOrder.burgerName)
        {
            Debug.Log("OrdersManager: Order completed! +" + currentOrder.reward + " points.");
            
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
                scoreManager.AddMoney(currentOrder.reward);
            else
                Debug.LogWarning("OrdersManager: ScoreManager not found in scene!");
            
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

        // 3. Mostrar los ingredientes din�micamente
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