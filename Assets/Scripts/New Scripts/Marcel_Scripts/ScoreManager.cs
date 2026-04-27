using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int moneyGoal = 100;
    public TextMeshProUGUI moneyText;

    private int currentMoney = 0;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
        Debug.Log("Money: " + currentMoney + " / " + moneyGoal);

        if (currentMoney >= moneyGoal)
            Win();
    }

    private void Win()
    {
        Debug.Log("YOU WIN!");
        Time.timeScale = 0f;
        // TODO: show win screen
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + currentMoney + " / $" + moneyGoal;
    }
}