using System.Collections.Generic;
using UnityEngine;

// Attach to a GameObject with a trigger Collider. When objects with the
// matching tag move inside the trigger, this zone deducts seconds from the
// scene Timer proportionally to the distance travelled inside the collider.
public class TimePenaltyZone : MonoBehaviour
{
    [Tooltip("Only affect objects with this tag. Empty = affect all colliders.")]
    public string targetTag = "Player";

    [Tooltip("Seconds removed from Timer per meter travelled inside the zone.")]
    public float timeLossPerMeter = 1f;

    [Tooltip("If true, applies penalty continuously while inside. If false, applies once on exit (based on accumulated distance).")]
    public bool applyContinuously = true;

    [Tooltip("If enabled, ensures the collider on this GameObject is a trigger.")]
    public bool enforceTrigger = true;

    public bool debug = true; // enable by default to aid debugging

    Timer timer;
    Collider zoneCollider;

    class TrackData { public Vector3 lastPos; public float accumulatedDistance; }
    Dictionary<Transform, TrackData> tracked = new Dictionary<Transform, TrackData>();
    // For polling overlap (works without Rigidbody on moving objects)
    HashSet<Transform> prevOverlapping = new HashSet<Transform>();

    void Awake()
    {
        timer = FindFirstObjectByType<Timer>();
        // Prefer collider on same GameObject, fall back to child colliders
        zoneCollider = GetComponent<Collider>();
        if (zoneCollider == null)
        {
            zoneCollider = GetComponentInChildren<Collider>();
            if (zoneCollider != null)
            {
                if (debug) Debug.Log($"TimePenaltyZone Awake: found child Collider '{zoneCollider.name}'");
            }
            else
            {
                Debug.LogWarning("TimePenaltyZone: No Collider found on this GameObject or its children. OnTrigger events will not fire.");
            }
        }

        if (zoneCollider != null && enforceTrigger)
        {
            zoneCollider.isTrigger = true;
        }

        if (debug) Debug.Log($"TimePenaltyZone Awake: targetTag='{targetTag}', timeLossPerMeter={timeLossPerMeter}, applyContinuously={applyContinuously}");
    }

    void OnEnable()
    {
        if (debug) Debug.Log($"TimePenaltyZone OnEnable: component enabled on GameObject '{gameObject.name}' active={gameObject.activeInHierarchy}");
    }

    void Start()
    {
        if (debug)
        {
            string colliderName = zoneCollider != null ? zoneCollider.name : "<none>";
            Debug.Log("TimePenaltyZone Start: zoneCollider=" + colliderName);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!MatchesTarget(other)) return;
        var t = other.transform;
        if (!tracked.ContainsKey(t))
        {
            bool wasEmpty = tracked.Count == 0;
            tracked[t] = new TrackData { lastPos = t.position, accumulatedDistance = 0f };
            if (wasEmpty && SoundManager.Instance != null)
                SoundManager.Instance.StartWarningLoop();
        }
        Debug.Log($"TimePenaltyZone: Enter - object='{other.name}', tag='{other.tag}', pos={other.transform.position}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!MatchesTarget(other)) return;
        var t = other.transform;
        if (tracked.TryGetValue(t, out var data))
        {
            if (!applyContinuously)
            {
                float penalty = data.accumulatedDistance * timeLossPerMeter;
                ApplyPenalty(penalty);
                Debug.Log($"TimePenaltyZone: Exit - object='{other.name}', accumulatedDistance={data.accumulatedDistance:F3}m, penalty={penalty:F2}s");
            }
            tracked.Remove(t);
            if (tracked.Count == 0 && SoundManager.Instance != null)
                SoundManager.Instance.StopWarningLoop();
        }
    }

    void Update()
    {
        // Poll overlapping colliders each frame so this works without a Rigidbody
        PollOverlapping();

        if (tracked.Count == 0) return;

        // Iterate over a copy to allow removal during the loop
        var entries = new List<KeyValuePair<Transform, TrackData>>(tracked);
        foreach (var kv in entries)
        {
            var t = kv.Key;
            var data = kv.Value;
            if (t == null)
            {
                tracked.Remove(t);
                if (tracked.Count == 0 && SoundManager.Instance != null)
                    SoundManager.Instance.StopWarningLoop();
                continue;
            }

            float dist = Vector3.Distance(t.position, data.lastPos);
            data.accumulatedDistance += dist;
            data.lastPos = t.position;

            if (applyContinuously && dist > 0f)
            {
                float penalty = dist * timeLossPerMeter;
                ApplyPenalty(penalty);
                Debug.Log($"TimePenaltyZone: Continuous penalty - object='{t.name}', dist={dist:F3}m, penalty={penalty:F3}s, totalAccum={data.accumulatedDistance:F3}m");
            }

            tracked[t] = data;
        }
    }

    // Poll physics for colliders inside this zone (does not require Rigidbodies on the moving objects)
    void PollOverlapping()
    {
        if (zoneCollider == null) return;

        Collider[] overlaps;
        // Use axis-aligned bounds as a fallback for arbitrary collider types
        var center = zoneCollider.bounds.center;
        var halfExtents = zoneCollider.bounds.extents;

        overlaps = Physics.OverlapBox(center, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

        var current = new HashSet<Transform>();
        foreach (var c in overlaps)
        {
            if (!MatchesTarget(c)) continue;
            if (c.transform == this.transform) continue; // ignore self
            current.Add(c.transform);
        }

        // Determine enters
        foreach (var t in current)
        {
            if (!prevOverlapping.Contains(t))
            {
                // Enter
                if (debug) Debug.Log($"TimePenaltyZone: POLL Enter - object='{t.name}'");
                if (!tracked.ContainsKey(t))
                {
                    bool wasEmpty = tracked.Count == 0;
                    tracked[t] = new TrackData { lastPos = t.position, accumulatedDistance = 0f };
                    if (wasEmpty && SoundManager.Instance != null)
                        SoundManager.Instance.StartWarningLoop();
                }
            }
        }

        // Determine exits
        var exited = new List<Transform>();
        foreach (var t in prevOverlapping)
        {
            if (!current.Contains(t)) exited.Add(t);
        }

        foreach (var t in exited)
        {
            if (tracked.TryGetValue(t, out var data))
            {
                if (!applyContinuously)
                {
                    float penalty = data.accumulatedDistance * timeLossPerMeter;
                    ApplyPenalty(penalty);
                    if (debug) Debug.Log($"TimePenaltyZone: POLL Exit - object='{t.name}', accumulatedDistance={data.accumulatedDistance:F3}m, penalty={penalty:F2}s");
                }
                tracked.Remove(t);
                if (tracked.Count == 0 && SoundManager.Instance != null)
                    SoundManager.Instance.StopWarningLoop();
            }
        }

        prevOverlapping = current;
    }

    bool MatchesTarget(Collider other)
    {
        if (string.IsNullOrEmpty(targetTag)) return true;
        return other.CompareTag(targetTag);
    }

    void ApplyPenalty(float seconds)
    {
        if (seconds <= 0f) return;
        if (timer == null)
        {
            timer = FindFirstObjectByType<Timer>();
            if (timer == null)
            {
                if (debug) Debug.LogWarning("TimePenaltyZone: No Timer found in scene to apply penalty.");
                return;
            }
        }

        timer.timeRemaining = Mathf.Max(0f, timer.timeRemaining - seconds);
        if (debug) Debug.Log($"TimePenaltyZone: applied penalty {seconds:F2}s -> timeRemaining={timer.timeRemaining:F2}s");
    }

    void OnDisable()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.StopWarningLoop();
    }
}
