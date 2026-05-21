using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public PlayerSpot player1Spot;
    public PlayerSpot player2Spot;

    private bool gameStarted = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
            Debug.Log("StartScreenManager: Called GameManager.ResetGame().");
        }
    }

    void Update()
    {
        if (!gameStarted &&
            player1Spot != null &&
            player2Spot != null &&
            player1Spot.occupied &&
            player2Spot.occupied)
        {
            gameStarted = true;
            StartGame();
        }
    }

    void StartGame()
    {
        Debug.Log("GAME START");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRound();
        }
        else
        {
            SceneManager.LoadScene("RaulScene");
        }
    }
}