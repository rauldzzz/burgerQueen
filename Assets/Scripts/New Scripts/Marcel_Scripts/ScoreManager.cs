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
        UpdateUI();
        Debug.Log($"ScoreManager: Awake, currentMoney=${currentMoney}.");
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
        Debug.Log($"ScoreManager: Added ${amount}, currentMoney=${currentMoney}.");
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