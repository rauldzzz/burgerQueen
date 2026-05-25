using UnityEngine;
using UnityEngine.UI; // �Importante a�adir esto para usar Image!
using System.Collections.Generic;
using TMPro;
using System;
using System.Reflection;

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

    public event Action CurrentOrderChanged;

    public BurgerRecipe CurrentOrder => currentOrder;

    void Start()
    {
        ApplyGameUpgrades();
        GenerateNewOrder();
    }

    private void ApplyGameUpgrades()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.unlockCheeseBurger)
        {
            List<BurgerRecipe> unlockedRecipes = GetUnlockedRecipesFromGameManager(GameManager.Instance);
            if (unlockedRecipes == null)
                return;

            int added = 0;
            foreach (BurgerRecipe r in unlockedRecipes)
            {
                if (r == null) continue;
                if (!possibleOrders.Contains(r))
                {
                    possibleOrders.Add(r);
                    added++;
                }
            }

            Debug.Log($"OrdersManager: Applied game upgrades - added {added} unlocked recipes.");
        }
    }

    private List<BurgerRecipe> GetUnlockedRecipesFromGameManager(GameManager gameManager)
    {
        if (gameManager == null)
            return null;

        Type gameManagerType = gameManager.GetType();

        PropertyInfo recipesProperty = gameManagerType.GetProperty("newRecipes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (recipesProperty != null && typeof(List<BurgerRecipe>).IsAssignableFrom(recipesProperty.PropertyType))
        {
            return recipesProperty.GetValue(gameManager) as List<BurgerRecipe>;
        }

        FieldInfo recipesField = gameManagerType.GetField("newRecipes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (recipesField != null && typeof(List<BurgerRecipe>).IsAssignableFrom(recipesField.FieldType))
        {
            return recipesField.GetValue(gameManager) as List<BurgerRecipe>;
        }

        MethodInfo recipesMethod = gameManagerType.GetMethod("GetUnlockedRecipes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (recipesMethod != null && typeof(List<BurgerRecipe>).IsAssignableFrom(recipesMethod.ReturnType))
        {
            return recipesMethod.Invoke(gameManager, null) as List<BurgerRecipe>;
        }

        return null;
    }

    public void GenerateNewOrder()
    {
        if (possibleOrders.Count == 0) return;

        currentOrder = possibleOrders[UnityEngine.Random.Range(0, possibleOrders.Count)];

        UpdateUI();
        Debug.Log("OrdersManager: New order generated: " + currentOrder.burgerName + " (reward=" + currentOrder.reward + ")");
        CurrentOrderChanged?.Invoke();
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

            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
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