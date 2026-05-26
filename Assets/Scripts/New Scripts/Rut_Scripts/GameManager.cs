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
    private const string PrefsUnlockCheeseBurgerKey = "BQ_UnlockCheeseBurger";
    private const string PrefsBetterPanKey = "BQ_BetterPan";
    private const string PrefsExtraCuttingZoneKey = "BQ_ExtraCuttingZone";
    private const string PrefsExtraServingZoneKey = "BQ_ExtraServingZone";

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
    [Tooltip("List of recipes unlocked by the NewRecipe upgrade (assign in inspector)")]
    public List<BurgerRecipe> newRecipes;

    public GameObject oldCounterToCuttingZone;
    public GameObject extraCuttingZoneObject;

    public GameObject oldCounterToServingZone;
    public GameObject extraServingZoneObject;

    public GameObject newPan;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
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

            ApplyGameplayUpgradesIfNeeded(SceneManager.GetActiveScene());
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
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
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
        UpgradeCache.Clear();
        ClearSavedUpgrades();
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

        Debug.Log($"ROUND START - Round {currentRound}/{totalRounds}");
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

    public void SaveGameplayUpgrades()
    {
        PlayerPrefs.SetInt(PrefsUnlockCheeseBurgerKey, unlockCheeseBurger ? 1 : 0);
        PlayerPrefs.SetInt(PrefsBetterPanKey, betterPan ? 1 : 0);
        PlayerPrefs.SetInt(PrefsExtraCuttingZoneKey, extraCuttingZone ? 1 : 0);
        PlayerPrefs.SetInt(PrefsExtraServingZoneKey, extraServingZone ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"GameManager: Saved gameplay upgrades -> cheese={unlockCheeseBurger}, pan={betterPan}, cutting={extraCuttingZone}, serving={extraServingZone}.");
    }

    public bool IsUpgradePurchased(UpgradeButton.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeButton.UpgradeType.NewRecipe:
                return unlockCheeseBurger;
            case UpgradeButton.UpgradeType.BetterPan:
                return betterPan;
            case UpgradeButton.UpgradeType.ExtraCuttingZone:
                return extraCuttingZone;
            case UpgradeButton.UpgradeType.ExtraServingZone:
                return extraServingZone;
            default:
                return false;
        }
    }

    public List<BurgerRecipe> GetUnlockedRecipes()
    {
        return newRecipes;
    }

    private void ClearSavedUpgrades()
    {
        PlayerPrefs.DeleteKey(PrefsUnlockCheeseBurgerKey);
        PlayerPrefs.DeleteKey(PrefsBetterPanKey);
        PlayerPrefs.DeleteKey(PrefsExtraCuttingZoneKey);
        PlayerPrefs.DeleteKey(PrefsExtraServingZoneKey);
        PlayerPrefs.Save();
        Debug.Log("GameManager: Cleared saved gameplay upgrades.");
    }

    private void ApplyGameplayUpgrades()
    {
        RebindGameplayZoneReferences();

        foreach (Counter_Grill grill in FindObjectsByType<Counter_Grill>(FindObjectsSortMode.None))
        {
            if (betterPan)
            {
                if (newPan != null)
                    newPan.SetActive(true);
                Debug.Log("GameManager: Better pan enabled.");
            }
            else
            {
                if (newPan != null)
                    newPan.SetActive(false);
                Debug.Log("GameManager: Better pan disabled.");
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

        OrdersManager ordersManager = FindAnyObjectByType<OrdersManager>();

        if (ordersManager != null && unlockCheeseBurger && newRecipes != null)
        {
            int added = 0;
            foreach (BurgerRecipe r in newRecipes)
            {
                if (r == null) continue;
                if (!ordersManager.possibleOrders.Contains(r))
                {
                    ordersManager.possibleOrders.Add(r);
                    added++;
                }
            }

            Debug.Log($"GameManager: Applied NewRecipe upgrade - added {added} recipes to OrdersManager.");
        }
    }

    private void RebindGameplayZoneReferences()
    {
        oldCounterToCuttingZone = FindSceneObjectByName("Old Counter to Cutting Zone", oldCounterToCuttingZone);
        extraCuttingZoneObject = FindSceneObjectByName("Extra Cutting Zone", extraCuttingZoneObject);
        oldCounterToServingZone = FindSceneObjectByName("Old Counter to Serving Zone", oldCounterToServingZone);
        extraServingZoneObject = FindSceneObjectByName("Extra Serving Zone", extraServingZoneObject);
        newPan = FindSceneObjectByName("Extra Pan", newPan);
    }

    private GameObject FindSceneObjectByName(string objectName, GameObject currentReference)
    {
        if (currentReference != null)
            return currentReference;

        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in sceneTransforms)
        {
            if (transform != null && transform.gameObject.scene.name == gameplaySceneName && transform.name == objectName)
                return transform.gameObject;
        }

        Debug.LogWarning($"GameManager: Could not rebind gameplay object '{objectName}' in scene '{gameplaySceneName}'.");
        return null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyGameplayUpgradesIfNeeded(scene);
    }

    private void ApplyGameplayUpgradesIfNeeded(Scene scene)
    {
        if (scene.name != gameplaySceneName)
        {
            return;
        }

        ApplyGameplayUpgrades();
    }
}
