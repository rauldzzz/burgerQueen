using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public List<string> activeOrders = new List<string>();
    public float spawnInterval = 5f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnOrder), 2f, spawnInterval);
    }

    void SpawnOrder()
    {
        activeOrders.Add("Burger");
        Debug.Log("Nova comanda: Burger");
    }

    public bool CompleteOrder(string orderName)
    {
        if (activeOrders.Contains(orderName))
        {
            activeOrders.Remove(orderName);
            Debug.Log("Completada!");
            GameManager.Instance.AddCoins(10);
            return true;
        }

        Debug.Log("Incorrecte!");
        return false;
    }
}
