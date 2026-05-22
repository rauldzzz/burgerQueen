using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeShopManager : MonoBehaviour
{
    public float continueHoldDuration = 1.5f;
    public TextMeshProUGUI moneyText;
    public Renderer continueRenderer;
    public Color continueReadyColor = Color.green;
    public string[] continueTriggerTags = new string[] { "Player" };
    public bool acceptAnyContinueCollider = false;

    private float continueHoldTime = 0f;
    private bool continuePlayerInside = false;
    private bool continueTriggered = false;
    private Color continueOriginalColor = Color.white;
    private UpgradeButton[] upgradeButtons;
    private int reservedMoney = 0;

    private void Start()
    {
        upgradeButtons = FindObjectsOfType<UpgradeButton>();
        Debug.Log($"UpgradeShopManager: Found {upgradeButtons.Length} upgrade buttons.");
        if (continueRenderer == null)
            continueRenderer = GetComponent<Renderer>();

        if (continueRenderer != null)
            continueOriginalColor = continueRenderer.material.color;

        if (moneyText == null)
            Debug.LogError("UpgradeShopManager: moneyText is not assigned in the inspector.");

        if (GameManager.Instance == null && ScoreManager.Instance == null)
            Debug.LogWarning("UpgradeShopManager: No money source found in scene. Upgrade shop will show $0.");

        UpdateMoneyText();
        LogSceneMoneyState();
    }

    private void LogSceneMoneyState()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string gmState = GameManager.Instance != null ? "present" : "missing";
        string scoreState = ScoreManager.Instance != null ? "present" : "missing";
        Debug.Log($"UpgradeShopManager: Scene={sceneName}, moneyTextAssigned={(moneyText != null)}, GameManager={gmState}, ScoreManager={scoreState}");
    }

    private void Update()
    {
        UpdateMoneyText();
        UpdateContinueColor();

        if (!continuePlayerInside || continueTriggered)
            return;

        continueHoldTime += Time.deltaTime;

        if (continueHoldTime >= continueHoldDuration)
        {
            continueTriggered = true;
            Debug.Log("UpgradeShopManager: Continue triggered, applying selected upgrades.");
            ApplySelectedUpgrades();
        }
    }

    private void UpdateMoneyText()
    {
        if (moneyText == null) return;

        if (GameManager.Instance != null)
            moneyText.text = "$" + GameManager.Instance.currentMoney;
        else if (ScoreManager.Instance != null)
            moneyText.text = "$" + ScoreManager.Instance.GetMoney();
        else if (GameManager.hasCachedMoney)
            moneyText.text = "$" + GameManager.cachedMoney;
        else
            moneyText.text = "$0";
    }

    private void UpdateContinueColor()
    {
        if (continueRenderer == null)
            return;

        if (continuePlayerInside)
        {
            float t = Mathf.Clamp01(continueHoldTime / continueHoldDuration);
            continueRenderer.material.color = Color.Lerp(continueOriginalColor, continueReadyColor, t);
        }
        else
        {
            continueRenderer.material.color = continueOriginalColor;
        }
    }

    private void ApplySelectedUpgrades()
    {
        int selectedTotal = 0;
        foreach (UpgradeButton button in upgradeButtons)
        {
            if (button != null && button.isSelected)
                selectedTotal += button.price;
        }
        // If we have a GameManager, use it directly
        if (GameManager.Instance != null)
        {
            Debug.Log($"UpgradeShopManager: Total selected upgrades cost = ${selectedTotal} (player has ${GameManager.Instance.currentMoney}).");
            if (selectedTotal > GameManager.Instance.currentMoney)
            {
                Debug.LogWarning("UpgradeShopManager: Not enough money to apply selected upgrades.");
                return;
            }

            foreach (UpgradeButton button in upgradeButtons)
            {
                if (button == null || !button.isSelected)
                    continue;

                Debug.Log($"UpgradeShopManager: Applying upgrade {button.upgradeType} (price=${button.price}).");
                switch (button.upgradeType)
                {
                    case UpgradeButton.UpgradeType.NewRecipe:
                        GameManager.Instance.unlockCheeseBurger = true;
                        break;
                    case UpgradeButton.UpgradeType.BetterPan:
                        GameManager.Instance.betterPan = true;
                        break;
                    case UpgradeButton.UpgradeType.ExtraCuttingZone:
                        GameManager.Instance.extraCuttingZone = true;
                        break;
                    case UpgradeButton.UpgradeType.ExtraServingZone:
                        GameManager.Instance.extraServingZone = true;
                        break;
                }
            }

            GameManager.Instance.SpendMoney(selectedTotal);
            Debug.Log($"UpgradeShopManager: Spending ${selectedTotal}. Remaining money=${GameManager.Instance.currentMoney}.");
            UpdateMoneyText();
            ResetSelections();
            Debug.Log($"UpgradeShopManager: BEFORE loading gameplay - GameManager.currentMoney={GameManager.Instance.currentMoney}, GameManager.totalMoney={GameManager.Instance.totalMoney}, currentRound={GameManager.Instance.currentRound}");
            GameManager.Instance.StartRound();
            Debug.Log($"UpgradeShopManager: Loading gameplay scene {GameManager.Instance.gameplaySceneName} after upgrades.");
            SceneManager.LoadScene(GameManager.Instance.gameplaySceneName);
        }
        else
        {
            // No GameManager in scene: write to UpgradeCache and deduct from ScoreManager if available
            Debug.Log($"UpgradeShopManager: GameManager missing. Caching selected upgrades and deducting from ScoreManager if present. Total=${selectedTotal}.");

            foreach (UpgradeButton button in upgradeButtons)
            {
                if (button == null || !button.isSelected)
                    continue;

                switch (button.upgradeType)
                {
                    case UpgradeButton.UpgradeType.NewRecipe:
                        UpgradeCache.unlockCheeseBurger = true;
                        break;
                    case UpgradeButton.UpgradeType.BetterPan:
                        UpgradeCache.betterPan = true;
                        break;
                    case UpgradeButton.UpgradeType.ExtraCuttingZone:
                        UpgradeCache.extraCuttingZone = true;
                        break;
                    case UpgradeButton.UpgradeType.ExtraServingZone:
                        UpgradeCache.extraServingZone = true;
                        break;
                }
            }

            UpgradeCache.pendingSpend += selectedTotal;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddMoney(-selectedTotal);
                Debug.Log($"UpgradeShopManager: Deducted ${selectedTotal} from ScoreManager (now ${ScoreManager.Instance.GetMoney()}).");
            }
            else
            {
                if (!GameManager.hasCachedMoney)
                    GameManager.hasCachedMoney = true;

                GameManager.cachedMoney -= selectedTotal;
                if (GameManager.cachedMoney < 0)
                    GameManager.cachedMoney = 0;
                Debug.Log($"UpgradeShopManager: Deducted ${selectedTotal} from cached money (now ${GameManager.cachedMoney}).");
                GameManager.SaveCachedMoney();
            }

            UpdateMoneyText();
            ResetSelections();

            Debug.Log($"UpgradeShopManager: BEFORE loading gameplay (fallback) - cachedMoney={GameManager.cachedMoney}, hasCachedMoney={GameManager.hasCachedMoney}");

            GameManager.resumeAfterUpgrade = true;
            if (!GameManager.hasCachedMoney)
                GameManager.hasCachedMoney = true;
            GameManager.SaveCachedMoney();
            GameManager.SaveResumeAfterUpgrade();

            // Load gameplay scene so that GameManager (if part of that scene) can wake up and apply the cached upgrades
            Debug.Log("UpgradeShopManager: Loading gameplay scene 'CanvisProbes' to continue (fallback without GameManager). ");
            SceneManager.LoadScene("CanvisProbes");
        }
    }

    private void ResetSelections()
    {
        reservedMoney = 0;
        foreach (UpgradeButton button in upgradeButtons)
        {
            if (button != null)
                button.ResetSelection();
        }
    }

    public bool CanReserve(int amount)
    {
        int available = 0;
        if (GameManager.Instance != null)
            available = GameManager.Instance.currentMoney;
        else if (ScoreManager.Instance != null)
            available = ScoreManager.Instance.GetMoney();
        else if (GameManager.hasCachedMoney)
            available = GameManager.cachedMoney;
        else
            available = 0;

        bool can = available - reservedMoney >= amount;
        Debug.Log($"UpgradeShopManager: CanReserve({amount}) => {can} (available=${available}, reserved=${reservedMoney}).");
        return can;
    }

    public bool Reserve(int amount)
    {
        if (!CanReserve(amount))
            return false;

        reservedMoney += amount;
        Debug.Log($"UpgradeShopManager: Reserved ${amount}. Total reserved=${reservedMoney}.");
        return true;
    }

    private bool IsContinueCollider(Collider other)
    {
        if (acceptAnyContinueCollider)
            return true;

        if (continueTriggerTags == null || continueTriggerTags.Length == 0)
            return other.CompareTag("Player");

        foreach (string tag in continueTriggerTags)
        {
            if (other.CompareTag(tag))
                return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsContinueCollider(other))
            return;

        continuePlayerInside = true;
        continueHoldTime = 0f;
        Debug.Log($"UpgradeShopManager: Player entered Continue zone with tag '{other.tag}'.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsContinueCollider(other))
            return;

        continuePlayerInside = false;
        continueHoldTime = 0f;
        Debug.Log($"UpgradeShopManager: Player exited Continue zone with tag '{other.tag}'.");
    }

    private void OnMouseDown()
    {
        Debug.Log("UpgradeShopManager: OnMouseDown on continue object.");
        ContinueShop();
    }

    public void ContinueShop()
    {
        if (continueTriggered)
        {
            Debug.Log("UpgradeShopManager: Continue already triggered, ignoring additional request.");
            return;
        }

        if (!continuePlayerInside)
        {
            Debug.LogWarning("UpgradeShopManager: Continue requested but player is not inside continue zone.");
            return;
        }

        continueTriggered = true;
        Debug.Log("UpgradeShopManager: ContinueShop invoked manually.");
        ApplySelectedUpgrades();
    }
}

