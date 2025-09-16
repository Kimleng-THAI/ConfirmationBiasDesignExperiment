// ==========================================
// LSLManager.cs - Enhanced for Real-time Streaming
// ==========================================

using UnityEngine;
using LSL;
using System;
using System.Collections.Generic;

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

    // LSL Outlets - one set only!
    private StreamOutlet markerOutlet;
    private StreamOutlet responseOutlet;  // For structured response data
    private StreamOutlet behavioralOutlet;

    // Stream info
    private StreamInfo markerInfo;
    private StreamInfo responseInfo;
    private StreamInfo behavioralInfo;

    // Logging flags
    public bool isLSLActive = false;
    private bool streamsInitialized = false;

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize streams ONCE
        if (!streamsInitialized)
        {
            InitializeLSLStreams();
            streamsInitialized = true;
        }
    }

    void InitializeLSLStreams()
    {
        try
        {
            string uniqueId = SystemInfo.deviceUniqueIdentifier;

            // 1. Event Marker Stream (string markers)
            markerInfo = new StreamInfo(
                "Unity_BCI_Markers",
                "Markers",
                1,
                0, // irregular rate
                channel_format_t.cf_string,
                "UnityBCI_Markers_" + uniqueId
            );

            var markerDesc = markerInfo.desc();
            markerDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            markerDesc.append_child_value("version", "2.0");

            markerOutlet = new StreamOutlet(markerInfo);

            // 2. Response Stream (JSON-formatted string for complex data)
            responseInfo = new StreamInfo(
                "Unity_Likert_Responses",
                "Responses",
                1,  // Single channel for JSON string
                0,
                channel_format_t.cf_string,  // Changed to string for JSON
                "UnityBCI_Response_" + uniqueId
            );

            var responseDesc = responseInfo.desc();
            responseDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            responseDesc.append_child_value("format", "JSON");

            responseOutlet = new StreamOutlet(responseInfo);

            // 3. Behavioral Data Stream (JSON-formatted string)
            behavioralInfo = new StreamInfo(
                "Unity_Behavioral_Data",
                "Behavioral",
                1,  // Single channel for JSON string
                0,
                channel_format_t.cf_string,  // Changed to string for JSON
                "UnityBCI_Behavior_" + uniqueId
            );

            var behaviorDesc = behavioralInfo.desc();
            behaviorDesc.append_child_value("manufacturer", "Unity_BCI_Experiment");
            behaviorDesc.append_child_value("format", "JSON");

            behavioralOutlet = new StreamOutlet(behavioralInfo);

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

    // ==== MARKER FUNCTIONS ====
    public void SendMarker(string markerText)
    {
        if (!isLSLActive || markerOutlet == null) return;

        try
        {
            string[] marker = new string[] { markerText };
            double timestamp = LSL.LSL.local_clock();
            markerOutlet.push_sample(marker, timestamp);

            Debug.Log($"[LSL Marker] {markerText} at {timestamp:F3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send marker: {e.Message}");
        }
    }

    // ==== STATEMENT RESPONSE (Phase 1) ====
    public void SendStatementResponse(int questionIndex, string topicCode, string statementCode,
                                     int agreement, float reactionTime,
                                     string attentionCheck = null, float attentionRT = 0f)
    {
        if (!isLSLActive || responseOutlet == null) return;

        try
        {
            // Create JSON object for the response
            var responseData = new
            {
                phase = 1,
                questionIndex = questionIndex,
                topicCode = topicCode,
                statementCode = statementCode,
                agreement = agreement,
                reactionTime = reactionTime,
                attentionCheck = attentionCheck,
                attentionCheckRT = attentionRT,
                timestamp = Time.time
            };

            string jsonData = JsonUtility.ToJson(responseData);
            string[] sample = new string[] { jsonData };
            double timestamp = LSL.LSL.local_clock();

            responseOutlet.push_sample(sample, timestamp);

            Debug.Log($"[LSL Response] Statement Q{questionIndex} {topicCode}-{statementCode}: Agreement={agreement}, RT={reactionTime:F3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send statement response: {e.Message}");
        }
    }

    // ==== ARTICLE RESPONSE (Phase 2) ====
    public void SendArticleResponse(string articleCode, string topicCode, int agreement,
                                   float reactionTime, float scrollDepth = 0f,
                                   string attentionCheck = null, float attentionRT = 0f)
    {
        if (!isLSLActive || responseOutlet == null) return;

        try
        {
            var responseData = new
            {
                phase = 2,
                articleCode = articleCode,
                topicCode = topicCode,
                agreement = agreement,
                reactionTime = reactionTime,
                scrollDepth = scrollDepth,
                attentionCheck = attentionCheck,
                attentionCheckRT = attentionRT,
                timestamp = Time.time
            };

            string jsonData = JsonUtility.ToJson(responseData);
            string[] sample = new string[] { jsonData };
            double timestamp = LSL.LSL.local_clock();

            responseOutlet.push_sample(sample, timestamp);

            Debug.Log($"[LSL Response] Article {articleCode}: Agreement={agreement}, RT={reactionTime:F3}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send article response: {e.Message}");
        }
    }

    // ==== BEHAVIORAL DATA ====
    public void SendBehavioralEvent(string eventType, Dictionary<string, object> eventData)
    {
        if (!isLSLActive || behavioralOutlet == null) return;

        try
        {
            // Add event type and timestamp to data
            eventData["eventType"] = eventType;
            eventData["timestamp"] = Time.time;
            eventData["realtime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            string jsonData = JsonUtility.ToJson(eventData);
            string[] sample = new string[] { jsonData };
            double timestamp = LSL.LSL.local_clock();

            behavioralOutlet.push_sample(sample, timestamp);

            Debug.Log($"[LSL Behavioral] {eventType}: {jsonData}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[LSL] Failed to send behavioral data: {e.Message}");
        }
    }

    // ==== SPECIALIZED BEHAVIORAL EVENTS ====
    public void SendArticleReadingBehavior(string articleCode, float dwellTime,
                                          float scrollDepth, int backButtonCount = 0)
    {
        var data = new Dictionary<string, object>
        {
            ["articleCode"] = articleCode,
            ["dwellTime"] = dwellTime,
            ["scrollDepth"] = scrollDepth,
            ["backButtonCount"] = backButtonCount
        };

        SendBehavioralEvent("ArticleReading", data);
    }

    public void SendTopicSelection(string topicName, int articlesReadInTopic)
    {
        var data = new Dictionary<string, object>
        {
            ["topic"] = topicName,
            ["articlesRead"] = articlesReadInTopic
        };

        SendBehavioralEvent("TopicSelection", data);
    }

    public void SendRestBreak(string breakType, float duration)
    {
        var data = new Dictionary<string, object>
        {
            ["breakType"] = breakType,
            ["duration"] = duration
        };

        SendBehavioralEvent("RestBreak", data);
    }

    // ==== LEGACY COMPATIBILITY (deprecate gradually) ====
    public void SendLikertResponse(int phase, int itemId, int rating, float responseTime)
    {
        // Route to appropriate new method based on phase
        if (phase == 1)
        {
            // For statements - need to extract topic/statement codes
            string topicCode = $"T{itemId / 100:D2}";
            string statementCode = $"S{itemId % 100:D2}";
            SendStatementResponse(itemId, topicCode, statementCode, rating, responseTime);
        }
        else if (phase == 2)
        {
            // For articles
            string articleCode = $"A{itemId:D3}";
            SendArticleResponse(articleCode, "", rating, responseTime);
        }
    }

    public void SendBehavioralData(int trial, int topicId, int articleId,
                                  int biasType, float scrollPos, float dwellTime)
    {
        // Convert to new format
        var data = new Dictionary<string, object>
        {
            ["trial"] = trial,
            ["topicId"] = topicId,
            ["articleId"] = articleId,
            ["biasType"] = biasType,
            ["scrollPosition"] = scrollPos,
            ["dwellTime"] = dwellTime
        };

        SendBehavioralEvent("LegacyBehavior", data);
    }

    // ==== CLEANUP ====
    void OnDestroy()
    {
        if (isLSLActive)
        {
            SendMarker("EXPERIMENT_END");

            // Clean up outlets
            markerOutlet = null;
            responseOutlet = null;
            behavioralOutlet = null;

            streamsInitialized = false;
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

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SendMarker("APPLICATION_LOST_FOCUS");
        else
            SendMarker("APPLICATION_GAINED_FOCUS");
    }
}