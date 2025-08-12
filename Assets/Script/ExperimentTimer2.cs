using UnityEngine;

public class ExperimentTimer2 : MonoBehaviour
{
    public static ExperimentTimer2 Instance;

    public float ExperimentStartTimeRealtime2 { get; private set; }

    private float restStartTime = 0f;
    private float totalRestDuration = 0f;
    private bool isResting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ExperimentStartTimeRealtime2 = Time.realtimeSinceStartup;
    }

    public void StartRest()
    {
        if (!isResting)
        {
            restStartTime = Time.realtimeSinceStartup;
            isResting = true;
        }
    }

    public void EndRest()
    {
        if (isResting)
        {
            totalRestDuration += Time.realtimeSinceStartup - restStartTime;
            isResting = false;
        }
    }

    public float GetGlobalTimestamp()
    {
        if (isResting)
        {
            // If currently resting, exclude rest time up to now
            return Time.realtimeSinceStartup - ExperimentStartTimeRealtime2 - (totalRestDuration + (Time.realtimeSinceStartup - restStartTime));
        }
        else
        {
            return Time.realtimeSinceStartup - ExperimentStartTimeRealtime2 - totalRestDuration;
        }
    }
}