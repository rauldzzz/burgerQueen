using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class OrdersManager : MonoBehaviour
{
    [System.Serializable]
    public class Order
    {
        public string dishName;
        public int reward;
    }

    [Header("Orders")]
    public List<Order> possibleOrders;
    public TextMeshProUGUI orderText;

    private Order currentOrder;

    void Start()
    {
        GenerateNewOrder();
    }

    public void GenerateNewOrder()
    {
        if (possibleOrders.Count == 0) return;
        currentOrder = possibleOrders[Random.Range(0, possibleOrders.Count)];
        UpdateUI();
        Debug.Log("New order: " + currentOrder.dishName);
    }

    public bool CompleteOrder(string dishName)
    {
        if (currentOrder == null) return false;

        if (dishName == currentOrder.dishName)
        {
            FindObjectOfType<ScoreManager>().AddMoney(currentOrder.reward);
            GenerateNewOrder();
            return true;
        }

        return false;
    }

    private void UpdateUI()
    {
        if (orderText != null)
            orderText.text = "Order: " + currentOrder.dishName;
    }
}