using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ArticleViewerManager : MonoBehaviour
{
    public TextMeshProUGUI topicText;
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI contentText;

    // Timestamp for current scene
    private float sceneStartTime;

    private List<string> requiredTopics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Economic Policy and Inequality",
        "Education and Learning Methods",
        "Health and Medical Approaches",
        "Technology and Social Media Impact"
    };

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        var tracker = ArticleSelectionTracker.Instance;

        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            topicText.text = lastArticle.topic;
            headlineText.text = lastArticle.headline;
            contentText.text = lastArticle.content;
        }
        else
        {
            topicText.text = "No Topic";
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
        }

        // Enable Continue only if 2 unique articles per topic have been read
        if (tracker != null && tracker.HasReadMinimumTwoArticlesPerTopic(requiredTopics))
        {
            Debug.Log("[ArticleViewerScene]: Continue enabled. All topics have 2 unique articles read.");
        }
        else
        {
            Debug.Log("[ArticleViewerScene]: Continue disabled. Participant hasn't read 2 unique articles per topic.");
        }
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.GoBack.performed += OnBackKeyPressed;
        inputActions.UI.GoForward.performed += OnForwardKeyPressed;
    }

    void OnDisable()
    {
        inputActions.UI.GoBack.performed -= OnBackKeyPressed;
        inputActions.UI.GoForward.performed -= OnForwardKeyPressed;
        inputActions.UI.Disable();
    }

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        LogEvent("BackButtonClicked");
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        // Only allow continue if participant has read minimum two articles per topic
        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.HasReadMinimumTwoArticlesPerTopic(requiredTopics))
        {
            LogEvent("ContinueButtonClicked");
            PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
            SceneManager.LoadScene("TransitionScene");
            Debug.Log("[ArticleViewerScene]: Participant has finished reading the article.");
        }
        else
        {
            Debug.Log("[ArticleViewerScene]: Continue action ignored. Participant hasn't read 2 unique articles per topic.");
        }
    }

    private void LogEvent(string label)
    {
        if (QuestionScreen.participantData != null)
        {
            float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
            float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"{label} (local: {localTimestamp:F2}s)"
            });

            Debug.Log($"[ArticleViewerScene] Event Logged: {label} | Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
        }
        else
        {
            Debug.LogWarning("[ArticleViewerScene] Could not log event: participantData is null.");
        }
    }
}