using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UpgradeShopManager : MonoBehaviour
{
    public float continueHoldDuration = 1.5f;
    public TextMeshProUGUI moneyText;
    public Renderer continueRenderer;
    public Color continueReadyColor = Color.green;

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

        UpdateMoneyText();
        Debug.Log($"UpgradeShopManager: Starting with player money=${GameManager.Instance?.totalMoney}");
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
            moneyText.text = "$" + GameManager.Instance.totalMoney;
        else if (ScoreManager.Instance != null)
            moneyText.text = "$" + ScoreManager.Instance.GetMoney();
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
            Debug.Log($"UpgradeShopManager: Total selected upgrades cost = ${selectedTotal} (player has ${GameManager.Instance.totalMoney}).");
            if (selectedTotal > GameManager.Instance.totalMoney)
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
            Debug.Log($"UpgradeShopManager: Spending ${selectedTotal}. Remaining money=${GameManager.Instance.totalMoney}.");
            UpdateMoneyText();
            ResetSelections();
            GameManager.Instance.StartRound();
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
                Debug.LogWarning("UpgradeShopManager: ScoreManager not present; could not deduct funds now.");
            }

            UpdateMoneyText();
            ResetSelections();

            // Load gameplay scene so that GameManager (if part of that scene) can wake up and apply the cached upgrades
            Debug.Log("UpgradeShopManager: Loading gameplay scene 'RaulScene' to continue.");
            SceneManager.LoadScene("RaulScene");
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
            available = GameManager.Instance.totalMoney;
        else if (ScoreManager.Instance != null)
            available = ScoreManager.Instance.GetMoney();
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        continuePlayerInside = true;
        continueHoldTime = 0f;
        Debug.Log("UpgradeShopManager: Player entered Continue zone.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        continuePlayerInside = false;
        continueHoldTime = 0f;
        Debug.Log("UpgradeShopManager: Player exited Continue zone.");
    }
}
