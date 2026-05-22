using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 120f; //120s = 2 min
    public TextMeshProUGUI timerText;

    private bool isRunning = true; //per parar quan arribi a 0

    void Update()
    {
        if (!isRunning) return; //si ja ha arribat a 0

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            isRunning = false;

            UpdateTimerDisplay(timeRemaining);
            TimeEnded();
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TimeEnded()
    {
        Debug.Log("Temps acabat!");

        if (GameManager.Instance != null && ScoreManager.Instance != null)
        {
            int gained = ScoreManager.Instance.GetMoney();
            GameManager.Instance.totalMoney += gained;
            GameManager.cachedMoney = GameManager.Instance.totalMoney;
            GameManager.hasCachedMoney = true;
            Debug.Log($"Timer: Round ended. Added ${gained} to total money (now ${GameManager.Instance.totalMoney}).");

            if (GameManager.Instance.currentRound >= GameManager.Instance.totalRounds)
            {
                GameManager.Instance.EndSession();
                return;
            }

            Debug.Log($"Timer: Loading upgrade scene {GameManager.Instance.upgradeSceneName}.");
            SceneManager.LoadScene(GameManager.Instance.upgradeSceneName);
        }
        else
        {
            SceneManager.LoadScene("UpgradeShop");
        }
    }
}