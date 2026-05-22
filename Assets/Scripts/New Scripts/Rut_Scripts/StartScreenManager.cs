using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public PlayerSpot player1Spot;
    public PlayerSpot player2Spot;
    public float startDelaySeconds = 2f;

    private bool gameStarted = false;
    private float readyTimer = 0f;

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
        if (gameStarted)
            return;

        bool bothReady =
            player1Spot != null &&
            player2Spot != null &&
            player1Spot.occupied &&
            player2Spot.occupied;

        if (bothReady)
        {
            readyTimer += Time.deltaTime;
            if (readyTimer >= startDelaySeconds)
            {
                gameStarted = true;
                StartGame();
            }
        }
        else
        {
            readyTimer = 0f;
        }
    }

    void StartGame()
    {
        Debug.Log("GAME START");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRound();
            Debug.Log($"StartScreenManager: Loading gameplay scene {GameManager.Instance.gameplaySceneName}.");
            SceneManager.LoadScene(GameManager.Instance.gameplaySceneName);
        }
        else
        {
            SceneManager.LoadScene("CanvisProbes");
        }
    }
}