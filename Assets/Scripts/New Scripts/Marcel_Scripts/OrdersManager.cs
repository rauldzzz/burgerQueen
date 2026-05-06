using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class OrdersManager : MonoBehaviour
{
    [Header("Orders")]
    // 1. Cambiamos la lista para que reciba archivos BurgerRecipe
    public List<BurgerRecipe> possibleOrders;
    public TextMeshProUGUI orderText;

    // 2. La comanda actual ahora es un objeto BurgerRecipe
    private BurgerRecipe currentOrder;

    void Start()
    {
        GenerateNewOrder(); 
    }

    public void GenerateNewOrder()
    {
        if (possibleOrders.Count == 0) return; 
        
        // 3. Elegimos una receta aleatoria de la lista
        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];
        
        UpdateUI();
        Debug.Log("New order: " + currentOrder.burgerName);
    }

    public bool CompleteOrder(string submittedBurgerName)
    {
        if (currentOrder == null) return false; 

        // 4. Comparamos el nombre
        if (submittedBurgerName == currentOrder.burgerName)
        {
            // Sumamos el dinero
            FindObjectOfType<ScoreManager>().AddMoney(currentOrder.reward);
            
            GenerateNewOrder(); 
            return true;
        }

        return false; 
    }

    private void UpdateUI()
    {
        if (orderText != null) 
            orderText.text = currentOrder.burgerName;
    }
}