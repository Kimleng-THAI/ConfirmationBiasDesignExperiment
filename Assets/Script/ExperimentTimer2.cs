using UnityEngine;

public class ExperimentTimer2 : MonoBehaviour
{
    public static ExperimentTimer2 Instance;

    public float ExperimentStartTimeRealtime2 { get; private set; }

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
        ExperimentStartTimeRealtime2 = Time.realtimeSinceStartup;
    }

    public float GetGlobalTimestamp()
    {
        return Time.realtimeSinceStartup - ExperimentStartTimeRealtime2;
    }
}
