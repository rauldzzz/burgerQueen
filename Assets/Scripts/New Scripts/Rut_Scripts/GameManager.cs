using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scenes")]
    public string initialSceneName = "Inici";
    public string gameplaySceneName = "RaulScene";
    public string upgradeSceneName = "UpgradeShop";

    [Header("Rounds")]
    public int totalRounds = 3;
    public int currentRound = 0;

    [Header("Money")]
    public int totalMoney = 0;

    [Header("Upgrades")]
    public bool unlockCheeseBurger = false;
    public bool betterPan = false;
    public bool extraCuttingZone = false;
    public bool extraServingZone = false;

    [Tooltip("How much faster the grill cooks after buying the better pan.")]
    public float grillSpeedMultiplier = 0.75f;

    [Header("Upgrade References")]
    public BurgerRecipe cheeseBurgerRecipe;

    public GameObject oldCounterToCuttingZone;
    public GameObject extraCuttingZoneObject;

    public GameObject oldCounterToServingZone;
    public GameObject extraServingZoneObject;    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager: instance created.");
            // If upgrades were purchased while GameManager wasn't present, apply them now
            if (UpgradeCache.HasPending())
            {
                Debug.Log("GameManager: Found pending upgrades in UpgradeCache, applying now.");
                UpgradeCache.ApplyTo(this);
            }
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("GameManager: duplicate instance destroyed.");
        }
    }

    public void ResetGame()
    {
        currentRound = 0;
        totalMoney = 0;
        unlockCheeseBurger = false;
        betterPan = false;
        extraCuttingZone = false;
        extraServingZone = false;
        Debug.Log("GameManager: Reset game state to defaults.");
    }

    public bool CanAfford(int price)
    {
        return totalMoney >= price;
    }

    public void SpendMoney(int amount)
    {
        totalMoney -= amount;
        if (totalMoney < 0)
            totalMoney = 0;
    }

    public void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            EndSession();
            return;
        }

        currentRound++;

        ApplyGameplayUpgrades();

        Debug.Log($"ROUND START - Round {currentRound}/{totalRounds}");
    }

    public void EndSession()
    {
        ResetGame();
        Debug.Log("GameManager: Ending session and returning to initial scene.");
        SceneManager.LoadScene(initialSceneName);
    }

    private void ApplyGameplayUpgrades()
    {
        foreach (Counter_Grill grill in FindObjectsOfType<Counter_Grill>())
        {
            if (betterPan)
            {
                grill.ApplyInteractDelayMultiplier(grillSpeedMultiplier);
                Debug.Log($"GameManager: Applied better pan (mult={grillSpeedMultiplier}) to {grill.name}.");
            }
            else
            {
                grill.ResetInteractDelay();
                Debug.Log($"GameManager: Reset interact delay on {grill.name}.");
            }
        }

        // CUTTING ZONE
        if (extraCuttingZone)
        {
            if (oldCounterToCuttingZone != null)
                oldCounterToCuttingZone.SetActive(false);

            if (extraCuttingZoneObject != null)
                extraCuttingZoneObject.SetActive(true);
            Debug.Log("GameManager: Extra cutting zone enabled.");
        }
        else
        {
            if (oldCounterToCuttingZone != null)
                oldCounterToCuttingZone.SetActive(true);

            if (extraCuttingZoneObject != null)
                extraCuttingZoneObject.SetActive(false);
            Debug.Log("GameManager: Extra cutting zone disabled.");
        }

        // SERVING ZONE
        if (extraServingZone)
        {
            if (oldCounterToServingZone != null)
                oldCounterToServingZone.SetActive(false);

            if (extraServingZoneObject != null)
                extraServingZoneObject.SetActive(true);
            Debug.Log("GameManager: Extra serving zone enabled.");
        }
        else
        {
            if (oldCounterToServingZone != null)
                oldCounterToServingZone.SetActive(true);

            if (extraServingZoneObject != null)
                extraServingZoneObject.SetActive(false);
            Debug.Log("GameManager: Extra serving zone disabled.");
        }

        OrdersManager ordersManager = FindObjectOfType<OrdersManager>();

        if (ordersManager != null &&
            unlockCheeseBurger &&
            cheeseBurgerRecipe != null)
        {
            if (!ordersManager.possibleOrders.Contains(cheeseBurgerRecipe))
            {
                ordersManager.possibleOrders.Add(cheeseBurgerRecipe);
                Debug.Log("GameManager: Cheese burger recipe added to OrdersManager.");
            }
            else
            {
                Debug.Log("GameManager: Cheese burger recipe already present in OrdersManager.");
            }
        }
    }
}
