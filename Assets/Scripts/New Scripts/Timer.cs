using UnityEngine;
using TMPro;

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

        Time.timeScale = 0f;

        //Es pot afegir pantalla game over, guardar puntuació, actualització monedes, etc
    }
}