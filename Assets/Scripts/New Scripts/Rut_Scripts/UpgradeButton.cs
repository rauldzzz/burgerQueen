using UnityEngine;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    public enum UpgradeType
    {
        NewRecipe,
        BetterPan,
        ExtraCuttingZone,
        ExtraServingZone
    }

    public UpgradeType upgradeType;
    public int price = 100;
    public float holdDuration = 2f;
    public TextMeshProUGUI priceText;
    public Renderer targetRenderer;
    public Color selectedColor = Color.green;
    public Color disabledColor = Color.red;
    public Color appliedColor = new Color(0.5f, 0.5f, 0.5f); // Grey for upgrades already applied
    [HideInInspector]
    public bool isSelected = false;

    private float currentHoldTime = 0f;
    private bool isPlayerInside = false;
    private bool isAlreadyApplied = false; // Track if the upgrade has already been applied
    private Color originalColor = Color.white;
    private UpgradeShopManager shopManager;
    private Renderer buttonRenderer;

    private void Start()
    {
        shopManager = FindFirstObjectByType<UpgradeShopManager>();
        buttonRenderer = targetRenderer != null ? targetRenderer : GetComponent<Renderer>();

        if (buttonRenderer != null)
            originalColor = buttonRenderer.material.color;

        // Ensure price text is assigned; try to find a TMP child as fallback
        if (priceText == null)
        {
            priceText = GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log($"UpgradeButton: priceText was null, found child TMP: {priceText != null} on {gameObject.name}.");
        }

        // If inspector left upgradeType as default for multiple buttons, try inferring from object name
        InferUpgradeTypeFromName();

        // Verificar si el upgrade ya ha sido aplicado
        CheckIfAlreadyApplied();

        Debug.Log($"UpgradeButton.Start: {gameObject.name} -> upgradeType={upgradeType}, price=${price}, shopManager={(shopManager!=null)}, alreadyApplied={isAlreadyApplied}.");

        SetPriceText();
        UpdateVisual();
    }

    private void CheckIfAlreadyApplied()
    {
        isAlreadyApplied = false;
        
        if (GameManager.Instance != null)
        {
            switch (upgradeType)
            {
                case UpgradeType.NewRecipe:
                    isAlreadyApplied = GameManager.Instance.unlockCheeseBurger;
                    break;
                case UpgradeType.BetterPan:
                    isAlreadyApplied = GameManager.Instance.betterPan;
                    break;
                case UpgradeType.ExtraCuttingZone:
                    isAlreadyApplied = GameManager.Instance.extraCuttingZone;
                    break;
                case UpgradeType.ExtraServingZone:
                    isAlreadyApplied = GameManager.Instance.extraServingZone;
                    break;
            }
        }
        
        if (isAlreadyApplied)
            Debug.Log($"UpgradeButton: {gameObject.name} ({upgradeType}) ya ha sido aplicado.");
    }

    private void SetPriceText()
    {
        if (priceText != null)
        {
            string label = GetReadableLabel();
            string statusText = isAlreadyApplied ? " (Applied)" : "";
            priceText.text = string.Format("{0}\n$ {1}{2}", label, price, statusText);
            Debug.Log($"UpgradeButton: SetPriceText on {gameObject.name} -> '{priceText.text}'.");
        }
    }

    private string GetReadableLabel()
    {
        switch (upgradeType)
        {
            case UpgradeType.NewRecipe: return "New Recipe";
            case UpgradeType.BetterPan: return "Better Pan";
            case UpgradeType.ExtraCuttingZone: return "Extra Cutting Zone";
            case UpgradeType.ExtraServingZone: return "Extra Serving Zone";
            default: return upgradeType.ToString();
        }
    }

    private void InferUpgradeTypeFromName()
    {
        string n = gameObject.name.ToLower();
        if (n.Contains("pan") || n.Contains("better") || n.Contains("grill"))
        {
            if (upgradeType != UpgradeType.BetterPan)
            {
                upgradeType = UpgradeType.BetterPan;
                Debug.Log($"UpgradeButton: Inferred BetterPan for {gameObject.name}.");
            }
        }
        else if (n.Contains("cut") || n.Contains("cort") || n.Contains("cutting"))
        {
            if (upgradeType != UpgradeType.ExtraCuttingZone)
            {
                upgradeType = UpgradeType.ExtraCuttingZone;
                Debug.Log($"UpgradeButton: Inferred ExtraCuttingZone for {gameObject.name}.");
            }
        }
        else if (n.Contains("serv") || n.Contains("safata") || n.Contains("serve"))
        {
            if (upgradeType != UpgradeType.ExtraServingZone)
            {
                upgradeType = UpgradeType.ExtraServingZone;
                Debug.Log($"UpgradeButton: Inferred ExtraServingZone for {gameObject.name}.");
            }
        }
        else if (n.Contains("new") || n.Contains("recipe") || n.Contains("cheese"))
        {
            if (upgradeType != UpgradeType.NewRecipe)
            {
                upgradeType = UpgradeType.NewRecipe;
                Debug.Log($"UpgradeButton: Inferred NewRecipe for {gameObject.name}.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            currentHoldTime = 0f;
            Debug.Log($"UpgradeButton: Player entered {gameObject.name} (upgrade={upgradeType}, price=${price}).");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            currentHoldTime = 0f;
            UpdateVisual();
            Debug.Log($"UpgradeButton: Player left {gameObject.name}.");
        }
    }

    private void Update()
    {
        // Si el upgrade ya fue aplicado, no hacer nada
        if (isAlreadyApplied)
            return;

        if (isSelected || !isPlayerInside)
            return;

        if (!CanReservePrice())
        {
            currentHoldTime = 0f;
            SetButtonColor(disabledColor);
            Debug.Log($"UpgradeButton: Not enough funds to reserve {upgradeType} (${price}).");
            return;
        }

        SetButtonColor(originalColor);
        currentHoldTime += Time.deltaTime;
        if (currentHoldTime >= holdDuration)
        {
            Debug.Log($"UpgradeButton: Hold complete on {gameObject.name}, attempting to reserve ${price}.");
            if (shopManager == null)
            {
                Debug.LogWarning($"UpgradeButton: shopManager is null on {gameObject.name}.");
            }
            else
            {
                bool reserved = shopManager.Reserve(price);
                Debug.Log($"UpgradeButton: Reserve returned {reserved} for ${price} on {gameObject.name}.");
                if (reserved)
                    Select();
            }

            currentHoldTime = 0f;
        }
    }

    private bool CanReservePrice()
    {
        if (shopManager == null)
            return false;

        return shopManager.CanReserve(price);
    }

    private void Select()
    {
        isSelected = true;
        SetButtonColor(selectedColor);
        Debug.Log($"UpgradeButton: Selected upgrade {upgradeType} for ${price} on {gameObject.name}.");
    }

    public void ResetSelection()
    {
        isSelected = false;
        currentHoldTime = 0f;
        UpdateVisual();
        Debug.Log($"UpgradeButton: Reset selection on {gameObject.name}.");
    }

    private void UpdateVisual()
    {
        if (isAlreadyApplied)
            SetButtonColor(appliedColor);
        else if (isSelected)
            SetButtonColor(selectedColor);
        else
            SetButtonColor(originalColor);
    }

    private void SetButtonColor(Color color)
    {
        if (buttonRenderer != null)
            buttonRenderer.material.color = color;
    }
}
