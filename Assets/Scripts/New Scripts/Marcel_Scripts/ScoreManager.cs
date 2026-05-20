using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Score Settings")]
    public TextMeshProUGUI moneyText;

    private int currentMoney = 0;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
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