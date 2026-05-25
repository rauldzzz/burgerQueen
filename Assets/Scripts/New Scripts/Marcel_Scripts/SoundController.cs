using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioClip pickUp;
    public AudioClip drop;
    public AudioClip cut;
    public AudioClip warning;
    public AudioClip grill;
    public AudioClip deliver;

    [Header("Debug")]
    public bool debugAudio = true;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Vector3 cameraPosition;
    private AudioSource warningLoopSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        cameraPosition = GetListenerPosition();
        warningLoopSource = gameObject.AddComponent<AudioSource>();
        warningLoopSource.loop = true;
        warningLoopSource.playOnAwake = false;
        warningLoopSource.spatialBlend = 0f;
        warningLoopSource.volume = sfxVolume;

        if (debugAudio)
        {
            int listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            Debug.Log("SoundManager Awake: listeners=" + listeners + ", cameraPos=" + cameraPosition + ", warningAssigned=" + (warning != null));
            if (listeners == 0)
                Debug.LogWarning("SoundManager: No AudioListener found in scene. You will not hear audio.");
            if (listeners > 1)
                Debug.LogWarning("SoundManager: More than one AudioListener found. Disable extras to avoid issues.");
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            if (debugAudio) Debug.LogWarning("SoundManager: Tried to play a null clip.");
            return;
        }

        cameraPosition = GetListenerPosition();
        AudioSource.PlayClipAtPoint(clip, cameraPosition, sfxVolume);
        if (debugAudio) Debug.Log("SoundManager: PlayClipAtPoint -> " + clip.name + " at " + cameraPosition + " volume=" + sfxVolume);
    }
    public void PlayPickUpClip()
    {
        PlaySound(pickUp);
    }

    public void PlayDropClip()
    {
        PlaySound(drop);
    }

    public void PlayCutClip()
    {
        PlaySound(cut);
    }
    public void PlayWarningClip() {
        PlaySound(warning); 
    }

    public void StartWarningLoop()
    {
        if (warning == null || warningLoopSource == null) return;
        if (warningLoopSource.isPlaying) return;

        warningLoopSource.clip = warning;
        warningLoopSource.volume = sfxVolume;
        warningLoopSource.Play();
        if (debugAudio) Debug.Log("SoundManager: Warning loop started.");
    }

    public void StopWarningLoop()
    {
        if (warningLoopSource == null) return;
        if (!warningLoopSource.isPlaying) return;

        warningLoopSource.Stop();
        if (debugAudio) Debug.Log("SoundManager: Warning loop stopped.");
    }

    public void PlayGrillClip() {
        PlaySound(grill); 
    }
    public void PlayDeliverClip() {
        PlaySound(deliver); 
    }

    private Vector3 GetListenerPosition()
    {
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        if (listener != null) return listener.transform.position;

        if (Camera.main != null) return Camera.main.transform.position;
        return transform.position;
    }

    [ContextMenu("Audio Test/Play Pickup")]
    private void TestPickup() => PlayPickUpClip();

    [ContextMenu("Audio Test/Play Drop")]
    private void TestDrop() => PlayDropClip();

    [ContextMenu("Audio Test/Play Cut")]
    private void TestCut() => PlayCutClip();

    [ContextMenu("Audio Test/Play Grill")]
    private void TestGrill() => PlayGrillClip();

    [ContextMenu("Audio Test/Play Deliver")]
    private void TestDeliver() => PlayDeliverClip();

    [ContextMenu("Audio Test/Start Warning Loop")]
    private void TestWarningStart() => StartWarningLoop();

    [ContextMenu("Audio Test/Stop Warning Loop")]
    private void TestWarningStop() => StopWarningLoop();
}