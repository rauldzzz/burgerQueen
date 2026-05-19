using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public PlayerSpot player1Spot;
    public PlayerSpot player2Spot;

    private bool gameStarted = false;

    void Update()
    {
        if (!gameStarted &&
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

        SceneManager.LoadScene("RaulScene");
    }
}