using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int cachedMoney = 0;
    public static bool hasCachedMoney = false;
    // When false, money will not be persisted across app restarts (PlayerPrefs disabled)
    public static bool persistMoney = false;
    public static bool resumeAfterUpgrade = false;

    private const string PrefsTotalMoneyKey = "BQ_TotalMoney";
    private const string PrefsHasCachedMoneyKey = "BQ_HasCachedMoney";
    private const string PrefsResumeAfterUpgradeKey = "BQ_ResumeAfterUpgrade";

    [Header("Scenes")]
    public string initialSceneName = "Inici";
    public string gameplaySceneName = "CanvisProbes";
    public string upgradeSceneName = "UpgradeShop";

    [Header("Rounds")]
    public int totalRounds = 3;
    public int currentRound = 0;

    [Header("Money")]
    public int totalMoney = 0;
    public int currentMoney = 0;

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

    private bool applyGameplayUpgradesOnNextGameplayLoad = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameManager: instance created and marked DontDestroyOnLoad.");

            if (persistMoney)
            {
                if (!hasCachedMoney && PlayerPrefs.HasKey(PrefsHasCachedMoneyKey))
                {
                    hasCachedMoney = PlayerPrefs.GetInt(PrefsHasCachedMoneyKey, 0) == 1;
                    cachedMoney = PlayerPrefs.GetInt(PrefsTotalMoneyKey, 0);
                    Debug.Log($"GameManager: Loaded cached money from PlayerPrefs=${cachedMoney}, hasCachedMoney={hasCachedMoney}.");
                }
            }

            if (hasCachedMoney)
            {
                currentMoney = cachedMoney;
                Debug.Log($"GameManager: Restored cached current money=${cachedMoney} on Awake.");
            }

            if (PlayerPrefs.GetInt(PrefsResumeAfterUpgradeKey, 0) == 1)
            {
                resumeAfterUpgrade = true;
                PlayerPrefs.SetInt(PrefsResumeAfterUpgradeKey, 0);
                PlayerPrefs.Save();
                Debug.Log("GameManager: Loaded resumeAfterUpgrade from PlayerPrefs.");
            }

            // If upgrades were purchased while GameManager wasn't present, apply them now
            if (UpgradeCache.HasPending())
            {
                Debug.Log("GameManager: Found pending upgrades in UpgradeCache, applying now.");
                UpgradeCache.ApplyTo(this);
            }

            if (resumeAfterUpgrade)
            {
                resumeAfterUpgrade = false;
                Debug.Log("GameManager: Resuming next round after upgrade shop.");
                StartRound();
            }
            Debug.Log($"GameManager: Awake finished -> totalMoney={totalMoney}, currentMoney={currentMoney}, cachedMoney={cachedMoney}, hasCachedMoney={hasCachedMoney}, currentRound={currentRound}");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("GameManager: duplicate instance destroyed.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ResetGame()
    {
        currentRound = 0;
        totalMoney = 0;
        currentMoney = 0;
        cachedMoney = 0;
        hasCachedMoney = false;
        resumeAfterUpgrade = false;
        ClearCachedMoney();
        unlockCheeseBurger = false;
        betterPan = false;
        extraCuttingZone = false;
        extraServingZone = false;
        Debug.Log("GameManager: Reset game state to defaults.");
    }

    public bool CanAfford(int price)
    {
        return currentMoney >= price;
    }

    public void SpendMoney(int amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0)
            currentMoney = 0;
        cachedMoney = currentMoney;
        hasCachedMoney = true;
        if (persistMoney) SaveCachedMoney();
    }

    public void AddEarnings(int amount)
    {
        if (amount <= 0) return;

        totalMoney += amount;
        currentMoney += amount;
        cachedMoney = currentMoney;
        hasCachedMoney = true;
        if (persistMoney) SaveCachedMoney();
    }

    public void StartRound()
    {
        if (currentRound >= totalRounds)
        {
            EndSession();
            return;
        }

        currentRound++;

        applyGameplayUpgradesOnNextGameplayLoad = true;

        Debug.Log($"GameManager: Round {currentRound}/{totalRounds} | unlockCheeseBurger={unlockCheeseBurger}, betterPan={betterPan}, extraCuttingZone={extraCuttingZone}, extraServingZone={extraServingZone}");

        // Upgrades are applied after the gameplay scene has finished loading, when its objects exist.
    }

    public void EndSession()
    {
        ResetGame();
        Debug.Log("GameManager: Ending session and returning to initial scene.");
        SceneManager.LoadScene(initialSceneName);
    }

    public static void SaveCachedMoney()
    {
        if (!persistMoney) return;
        PlayerPrefs.SetInt(PrefsTotalMoneyKey, cachedMoney);
        PlayerPrefs.SetInt(PrefsHasCachedMoneyKey, hasCachedMoney ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameManager: Saved cached money=${cachedMoney} to PlayerPrefs.");
    }

    public static void SaveResumeAfterUpgrade()
    {
        PlayerPrefs.SetInt(PrefsResumeAfterUpgradeKey, resumeAfterUpgrade ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameManager: Saved resumeAfterUpgrade={resumeAfterUpgrade} to PlayerPrefs.");
    }

    public static void ClearCachedMoney()
    {
        if (!persistMoney)
        {
            // If persistence is disabled, just clear runtime values
            cachedMoney = 0;
            hasCachedMoney = false;
            resumeAfterUpgrade = false;
            Debug.Log("GameManager: Persistence disabled; cleared runtime cached money.");
            return;
        }

        PlayerPrefs.DeleteKey(PrefsTotalMoneyKey);
        PlayerPrefs.DeleteKey(PrefsHasCachedMoneyKey);
        PlayerPrefs.DeleteKey(PrefsResumeAfterUpgradeKey);
        PlayerPrefs.Save();
        Debug.Log("GameManager: Cleared cached money PlayerPrefs.");
    }

    private void ApplyGameplayUpgrades()
    {
        RefreshGameplayReferences();

        foreach (Counter_Grill grill in FindObjectsByType<Counter_Grill>(FindObjectsSortMode.None))
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

        OrdersManager ordersManager = FindFirstObjectByType<OrdersManager>();

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!applyGameplayUpgradesOnNextGameplayLoad)
            return;

        if (scene.name != gameplaySceneName)
            return;

        applyGameplayUpgradesOnNextGameplayLoad = false;
        ApplyGameplayUpgrades();
    }

    private void RefreshGameplayReferences()
    {
        Counter[] counters = FindObjectsByType<Counter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Counter counter in counters)
        {
            if (counter == null)
                continue;

            string counterName = counter.gameObject.name;
            if (counterName == "Old Counter to Cutting Zone")
                oldCounterToCuttingZone = counter.gameObject;
            else if (counterName == "Extra Cutting Zone")
                extraCuttingZoneObject = counter.gameObject;
            else if (counterName == "Old Counter to Serving Zone")
                oldCounterToServingZone = counter.gameObject;
            else if (counterName == "Extra Serving Zone")
                extraServingZoneObject = counter.gameObject;
        }
    }
}
