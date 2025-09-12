// ==========================================
// LSLManager.cs - Core LSL Management
// ==========================================
using UnityEngine;
using LSL;
using System;

public class LSLManager : MonoBehaviour
{
    private static LSLManager instance;
    public static LSLManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LSLManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("LSLManager");
                    instance = go.AddComponent<LSLManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    // LSL Outlets
    private StreamOutlet markerOutlet;
    private StreamOutlet likertOutlet;
    private StreamOutlet behaviorOutlet;

    // Stream info
    private StreamInfo markerInfo;
    private StreamInfo likertInfo;
    private StreamInfo behaviorInfo;

    // Logging flag
    public bool isLSLActive = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeLSLStreams();
    }

    void InitializeLSLStreams()
    {
        try
        {
            // Initialize Event Marker Stream
            markerInfo = new StreamInfo(
                "Unity_BCI_Markers",
                "Markers",
                1,
                0, // irregular sampling rate
                channel_format_t.cf_string,
                "UnityBCI_Markers_" + SystemInfo.deviceUniqueIdentifier
            );

            var markerDesc = markerInfo.desc();
            markerDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            markerDesc.append_child_value("version", "1.0");
            var markerChannels = markerDesc.append_child("channels");
            markerChannels.append_child("channel")
                .append_child_value("label", "EventMarker")
                .append_child_value("type", "Event");

            markerOutlet = new StreamOutlet(markerInfo);

            // Initialize Likert Response Stream
            likertInfo = new StreamInfo(
                "Unity_Likert_Responses",
                "Responses",
                4,
                0,
                channel_format_t.cf_float32,
                "UnityBCI_Likert_" + SystemInfo.deviceUniqueIdentifier
            );

            var likertDesc = likertInfo.desc();
            likertDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            var likertChannels = likertDesc.append_child("channels");

            likertChannels.append_child("channel").append_child_value("label", "Phase");
            likertChannels.append_child("channel").append_child_value("label", "ItemID");
            likertChannels.append_child("channel").append_child_value("label", "Rating");
            likertChannels.append_child("channel").append_child_value("label", "ResponseTime");

            likertOutlet = new StreamOutlet(likertInfo);

            // Initialize Behavioral Data Stream
            behaviorInfo = new StreamInfo(
                "Unity_Behavioral_Data",
                "Behavioral",
                6,
                0,
                channel_format_t.cf_float32,
                "UnityBCI_Behavior_" + SystemInfo.deviceUniqueIdentifier
            );

            var behaviorDesc = behaviorInfo.desc();
            behaviorDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            var behaviorChannels = behaviorDesc.append_child("channels");

            behaviorChannels.append_child("channel").append_child_value("label", "TrialNumber");
            behaviorChannels.append_child("channel").append_child_value("label", "TopicID");
            behaviorChannels.append_child("channel").append_child_value("label", "ArticleID");
            behaviorChannels.append_child("channel").append_child_value("label", "BiasType");
            behaviorChannels.append_child("channel").append_child_value("label", "ScrollPosition");
            behaviorChannels.append_child("channel").append_child_value("label", "DwellTime");

            behaviorOutlet = new StreamOutlet(behaviorInfo);

            isLSLActive = true;
            Debug.Log("[LSL] All streams initialized successfully");
            SendMarker("LSL_STREAMS_INITIALIZED");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to initialize streams: {e.Message}");
            isLSLActive = false;
        }
    }

    public void SendMarker(string markerText)
    {
        if (!isLSLActive || markerOutlet == null) return;

        try
        {
            string[] marker = new string[] { markerText };
            double timestamp = LSL.LSL.local_clock(); // ✅ fixed
            markerOutlet.push_sample(marker, timestamp);
            Debug.Log($"[LSL] Marker sent: {markerText} at {timestamp:F3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send marker: {e.Message}");
        }
    }

    public void SendLikertResponse(int phase, int itemId, int rating, float responseTime)
    {
        if (!isLSLActive || likertOutlet == null) return;

        try
        {
            float[] sample = new float[] { phase, itemId, rating, responseTime };
            double timestamp = LSL.LSL.local_clock(); // ✅ fixed
            likertOutlet.push_sample(sample, timestamp);
            Debug.Log($"[LSL] Likert response sent: Phase={phase}, Item={itemId}, Rating={rating}, RT={responseTime:F3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send Likert response: {e.Message}");
        }
    }

    public void SendBehavioralData(int trial, int topicId, int articleId, int biasType, float scrollPos, float dwellTime)
    {
        if (!isLSLActive || behaviorOutlet == null) return;

        try
        {
            float[] sample = new float[] { trial, topicId, articleId, biasType, scrollPos, dwellTime };
            double timestamp = LSL.LSL.local_clock(); // ✅ fixed
            behaviorOutlet.push_sample(sample, timestamp);
            Debug.Log($"[LSL] Behavioral data sent: Trial={trial}, Topic={topicId}, Article={articleId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send behavioral data: {e.Message}");
        }
    }

    void OnDestroy()
    {
        if (isLSLActive)
        {
            SendMarker("EXPERIMENT_END");

            // ✅ No more close_stream(), just null them
            markerOutlet = null;
            likertOutlet = null;
            behaviorOutlet = null;

            Debug.Log("[LSL] All streams closed");
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SendMarker("APPLICATION_PAUSED");
        else
            SendMarker("APPLICATION_RESUMED");
    }
}