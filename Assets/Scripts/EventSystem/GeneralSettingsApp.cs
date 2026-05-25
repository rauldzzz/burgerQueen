using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GeneralSettingsApp : MonoBehaviour
{
    private static GeneralSettingsApp Instance;
    private AudioListener fallbackAudioListener;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 90; // o el refresh del HMD o Screen.currentResolution.refreshRate

        EnforceSingleSystems();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnforceSingleSystems();
    }

    private void EnforceSingleSystems()
    {
        EnsureSingleEventSystem();
        EnsureSingleAudioListener();
    }

    private void EnsureSingleEventSystem()
    {
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (systems.Length <= 1)
            return;

        EventSystem keep = EventSystem.current != null ? EventSystem.current : systems[0];
        foreach (EventSystem system in systems)
        {
            if (system == keep)
                continue;

            system.gameObject.SetActive(false);
        }
    }

    private void EnsureSingleAudioListener()
    {
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (listeners.Length == 0)
        {
            EnsureFallbackAudioListener();
            return;
        }

        AudioListener keep = null;
        if (Camera.main != null)
            keep = Camera.main.GetComponent<AudioListener>();

        if (keep == null)
            keep = listeners[0];

        foreach (AudioListener listener in listeners)
        {
            listener.enabled = listener == keep;
        }
    }

    private void EnsureFallbackAudioListener()
    {
        if (fallbackAudioListener == null)
        {
            GameObject fallbackObject = new GameObject("Persistent Audio Listener");
            fallbackObject.transform.SetParent(transform);
            fallbackObject.transform.localPosition = Vector3.zero;
            fallbackObject.transform.localRotation = Quaternion.identity;
            fallbackAudioListener = fallbackObject.AddComponent<AudioListener>();
            DontDestroyOnLoad(fallbackObject);
        }

        fallbackAudioListener.enabled = true;

        if (Camera.main != null)
        {
            fallbackAudioListener.transform.position = Camera.main.transform.position;
            fallbackAudioListener.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
