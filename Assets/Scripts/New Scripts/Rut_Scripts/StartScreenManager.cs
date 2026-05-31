using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartScreenManager : MonoBehaviour
{
    private const int DefaultVideoTextureWidth = 1920;
    private const int DefaultVideoTextureHeight = 1080;

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

    [Header("Start Screen Videos")]
    [Tooltip("RawImages for the start-screen videos. Add them in the same order you want them moved together.")]
    public List<RawImage> videoRawImages = new List<RawImage>();

    private bool gameStarted = false;
    private bool startSequenceRunning = false;
    private Vector3 instructionsStartLocalPosition;
    private bool hasInstructionsStartPosition = false;
    private readonly List<Vector3> videoRawImageStartLocalPositions = new List<Vector3>();
    private readonly List<RenderTexture> allocatedVideoTextures = new List<RenderTexture>();

    void Start()
    {
        CacheInstructionsStartPosition();
        CacheVideoRawImageStartPositions();
        ConfigureVideoPlayers();

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
                UpdateVideoRawImagePositions(movedDistance);
                yield return null;
            }

            instructionsText.localPosition = instructionsStartLocalPosition + new Vector3(0f, 0f, instructionsForwardDistance);
            UpdateVideoRawImagePositions(instructionsForwardDistance);
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

    private void ConfigureVideoPlayers()
    {
        VideoPlayer[] videoPlayers = FindObjectsOfType<VideoPlayer>(true);

        foreach (VideoPlayer videoPlayer in videoPlayers)
        {
            if (videoPlayer == null || videoPlayer.renderMode != VideoRenderMode.RenderTexture)
            {
                continue;
            }

            RenderTexture videoTexture = CreateVideoTexture(videoPlayer.name);
            videoPlayer.targetTexture = videoTexture;

            RawImage rawImage = FindAssociatedRawImage(videoPlayer);
            if (rawImage != null)
            {
                rawImage.texture = videoTexture;
            }
        }
    }

    private void CacheVideoRawImageStartPositions()
    {
        if (videoRawImageStartLocalPositions.Count > 0)
        {
            return;
        }

        for (int index = 0; index < videoRawImages.Count; index++)
        {
            RawImage videoRawImage = videoRawImages[index];
            if (videoRawImage == null)
            {
                continue;
            }

            videoRawImageStartLocalPositions.Add(videoRawImage.rectTransform.localPosition);
        }
    }

    private void UpdateVideoRawImagePositions(float movedDistance)
    {
        for (int index = 0; index < videoRawImages.Count; index++)
        {
            RawImage videoRawImage = videoRawImages[index];
            if (videoRawImage == null)
            {
                continue;
            }

            RectTransform videoRectTransform = videoRawImage.rectTransform;
            Vector3 startPosition = index < videoRawImageStartLocalPositions.Count
                ? videoRawImageStartLocalPositions[index]
                : videoRectTransform.localPosition;

            videoRectTransform.localPosition = startPosition + new Vector3(0f, 0f, movedDistance);
        }
    }

    private static RawImage FindAssociatedRawImage(VideoPlayer videoPlayer)
    {
        RawImage rawImage = videoPlayer.GetComponent<RawImage>();
        if (rawImage != null)
        {
            return rawImage;
        }

        rawImage = videoPlayer.GetComponentInChildren<RawImage>(true);
        if (rawImage != null)
        {
            return rawImage;
        }

        Transform parent = videoPlayer.transform.parent;
        while (parent != null)
        {
            rawImage = parent.GetComponentInChildren<RawImage>(true);
            if (rawImage != null)
            {
                return rawImage;
            }

            parent = parent.parent;
        }

        return null;
    }

    private RenderTexture CreateVideoTexture(string videoName)
    {
        RenderTexture videoTexture = new RenderTexture(DefaultVideoTextureWidth, DefaultVideoTextureHeight, 0, RenderTextureFormat.ARGB32)
        {
            name = $"{videoName}_VideoTexture",
            hideFlags = HideFlags.DontSave
        };

        videoTexture.Create();
        allocatedVideoTextures.Add(videoTexture);
        return videoTexture;
    }

    private void OnDestroy()
    {
        for (int index = 0; index < allocatedVideoTextures.Count; index++)
        {
            RenderTexture videoTexture = allocatedVideoTextures[index];
            if (videoTexture != null)
            {
                videoTexture.Release();
                Destroy(videoTexture);
            }
        }

        allocatedVideoTextures.Clear();
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