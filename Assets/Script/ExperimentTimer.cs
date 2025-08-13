using UnityEngine;

public class ExperimentTimer : MonoBehaviour
{
    public static ExperimentTimer Instance;

    public float ExperimentStartTimeRealtime { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Set the start time once
        ExperimentStartTimeRealtime = Time.realtimeSinceStartup;
    }

    public float GetGlobalTimestamp()
    {
        return Time.realtimeSinceStartup - ExperimentStartTimeRealtime;
    }

    public void AddToExperimentTime(float delta)
    {
        ExperimentStartTimeRealtime += delta;
    }
}