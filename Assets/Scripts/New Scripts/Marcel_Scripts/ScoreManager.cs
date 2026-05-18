using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public TextMeshProUGUI moneyText;

    private int currentMoney = 0;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();

    
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + currentMoney;
    }
}