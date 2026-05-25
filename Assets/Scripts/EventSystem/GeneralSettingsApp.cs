using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GeneralSettingsApp : MonoBehaviour
{
    private static GeneralSettingsApp Instance;

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
        EventSystem[] systems = FindObjectsOfType<EventSystem>(true);
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
        AudioListener[] listeners = FindObjectsOfType<AudioListener>(true);
        if (listeners.Length <= 1)
            return;

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
}
