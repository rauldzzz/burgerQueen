using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public TextMeshProUGUI moneyText;

    private int currentMoney = 0;

    private void Awake()
    {
        Instance = this;
        RestoreMoneyFromGameManager();
        UpdateUI();
        Debug.Log($"ScoreManager: Awake, currentMoney=${currentMoney}.");
    }

    public void AddMoney(int amount)
    {
        if (GameManager.Instance != null)
        {
            if (amount > 0)
            {
                GameManager.Instance.AddEarnings(amount);
            }
            else if (amount < 0)
            {
                GameManager.Instance.SpendMoney(-amount);
            }

            currentMoney = GameManager.Instance.currentMoney;
        }
        else
        {
            currentMoney += amount;
            if (currentMoney < 0)
            {
                currentMoney = 0;
            }

            GameManager.cachedMoney = currentMoney;
            GameManager.hasCachedMoney = true;
            GameManager.SaveCachedMoney();
        }

        UpdateUI();
        Debug.Log($"ScoreManager: Added ${amount}, currentMoney=${currentMoney}.");
    }

    private void RestoreMoneyFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            currentMoney = GameManager.Instance.currentMoney;
            return;
        }

        if (GameManager.hasCachedMoney)
        {
            currentMoney = GameManager.cachedMoney;
            return;
        }
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + currentMoney;
    }
}