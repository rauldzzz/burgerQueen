using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenManager : MonoBehaviour
{
    public PlayerSpot player1Spot;
    public PlayerSpot player2Spot;

    [Header("Start Screen UI")]
    public GameObject logo;
    public GameObject positionPlayer1;
    public GameObject positionPlayer2;
    public GameObject textLegacy;
    public GameObject textTmp;
    public GameObject textTmpAlt;
    public Transform instructionsText;

    [Tooltip("How fast the instructions move forward on the Z axis while they are shown.")]
    public float instructionsForwardSpeed = 3f;

    [Tooltip("How long, in seconds, the instructions stay visible after the movement starts.")]
    public float instructionsReadTime = 3f;

    [Tooltip("How far the instructions move forward on the Z axis before the game starts.")]
    public float instructionsForwardDistance = 12f;

    private bool gameStarted = false;
    private bool startSequenceRunning = false;
    private Vector3 instructionsStartLocalPosition;
    private bool hasInstructionsStartPosition = false;

    void Start()
    {
        CacheInstructionsStartPosition();

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

        if (bothReady && !startSequenceRunning)
        {
            startSequenceRunning = true;
            StartCoroutine(BeginStartSequence());
        }

        if (startSequenceRunning)
        {
            return;
        }
    }

    private IEnumerator BeginStartSequence()
    {
        SetStartScreenVisible(false);

        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(true);

            float movedDistance = 0f;
            float moveDuration = instructionsForwardSpeed > 0f ? instructionsForwardDistance / instructionsForwardSpeed : 0f;

            while (movedDistance < instructionsForwardDistance)
            {
                float step = instructionsForwardSpeed * Time.deltaTime;
                movedDistance = Mathf.Min(movedDistance + step, instructionsForwardDistance);
                instructionsText.localPosition = instructionsStartLocalPosition + new Vector3(0f, 0f, movedDistance);
                yield return null;
            }

            instructionsText.localPosition = instructionsStartLocalPosition + new Vector3(0f, 0f, instructionsForwardDistance);
        }

        if (instructionsReadTime > 0f)
        {
            yield return new WaitForSeconds(instructionsReadTime);
        }

        gameStarted = true;
        StartGame();
    }

    private void CacheInstructionsStartPosition()
    {
        if (instructionsText == null || hasInstructionsStartPosition)
            return;

        instructionsStartLocalPosition = instructionsText.localPosition;
        hasInstructionsStartPosition = true;
    }

    private void SetStartScreenVisible(bool visible)
    {
        SetActiveIfNotNull(logo, visible);
        SetActiveIfNotNull(positionPlayer1, visible);
        SetActiveIfNotNull(positionPlayer2, visible);
        SetActiveIfNotNull(textLegacy, visible);
        SetActiveIfNotNull(textTmp, visible);
        SetActiveIfNotNull(textTmpAlt, visible);
    }

    private static void SetActiveIfNotNull(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
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