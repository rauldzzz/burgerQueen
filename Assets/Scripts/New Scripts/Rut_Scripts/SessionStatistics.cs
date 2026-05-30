using UnityEngine;
using System.Collections.Generic;

public class SessionStatistics : MonoBehaviour
{
    public static SessionStatistics Instance;

    public int totalMoneyEarned;
    public int totalMoneySpent;

    private Dictionary<string, int> burgersPrepared =
        new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddBurger(string burgerName)
    {
        if (!burgersPrepared.ContainsKey(burgerName))
            burgersPrepared[burgerName] = 0;

        burgersPrepared[burgerName]++;
        Debug.Log($"Burger registrada: {burgerName}");

    }

    public int GetBurgerCount(string burgerName)
    {
        return burgersPrepared.ContainsKey(burgerName)
            ? burgersPrepared[burgerName]
            : 0;
    }

    public int GetTotalBurgers()
    {
        int total = 0;

        foreach (var burger in burgersPrepared)
            total += burger.Value;

        return total;
    }

    public void AddMoneyEarned(int amount)
    {
        totalMoneyEarned += amount;
        Debug.Log($"Money earned +{amount} | Total = {totalMoneyEarned}");
    }

    public void AddMoneySpent(int amount)
    {
        totalMoneySpent += amount;
        Debug.Log($"Money spent +{amount} | Total = {totalMoneySpent}");
    }

    public int GetRemainingMoney()
    {
        if (GameManager.Instance != null)
            return GameManager.Instance.currentMoney;

        return totalMoneyEarned - totalMoneySpent;

    }

    public Dictionary<string, int> GetAllBurgerCounts()
    {
        return burgersPrepared;
    }

    public void ResetStats()
    {
        totalMoneyEarned = 0;
        totalMoneySpent = 0;
        burgersPrepared.Clear();
    }
}