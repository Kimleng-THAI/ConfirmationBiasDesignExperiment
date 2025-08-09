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
    public TextMeshProUGUI agreementPromptText;

    // Timestamp for current scene
    private float sceneStartTime;

    // Track if participant already gave a response
    private bool hasResponded = false;

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

            // Show the agreement prompt
            if (agreementPromptText != null)
            {
                agreementPromptText.text = "How much do you agree or disagree with the above article?\n" +
                                           "1 - Strongly Disagree\n" +
                                           "2 - Disagree\n" +
                                           "3 - Neutral\n" +
                                           "4 - Agree\n" +
                                           "5 - Strongly Agree";
            }
        }
        else
        {
            topicText.text = "No Topic";
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
            if (agreementPromptText != null) agreementPromptText.text = "";
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
        inputActions.UI.Select1.performed += OnSelect1;
        inputActions.UI.Select2.performed += OnSelect2;
        inputActions.UI.Select3.performed += OnSelect3;
        inputActions.UI.Select4.performed += OnSelect4;
        inputActions.UI.Select5.performed += OnSelect5;
    }

    void OnDisable()
    {
        inputActions.UI.GoBack.performed -= OnBackKeyPressed;
        inputActions.UI.GoForward.performed -= OnForwardKeyPressed;
        inputActions.UI.Select1.performed -= OnSelect1;
        inputActions.UI.Select2.performed -= OnSelect2;
        inputActions.UI.Select3.performed -= OnSelect3;
        inputActions.UI.Select4.performed -= OnSelect4;
        inputActions.UI.Select5.performed -= OnSelect5;
        inputActions.UI.Disable();
    }

    private void OnSelect1(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("1");
    private void OnSelect2(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("2");
    private void OnSelect3(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("3");
    private void OnSelect4(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("4");
    private void OnSelect5(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("5");

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

    private void OnAgreementKeyPressed(string option)
    {
        // Ignore multiple responses (store only the first input)
        if (hasResponded) return;

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            lastArticle.selectedOption = option;
            hasResponded = true;

            LogEvent($"AgreementSelected: {option}", lastArticle.headline);

            Debug.Log($"[ArticleViewerScene]: Agreement response recorded: {option}");
        }
    }

    private void LogEvent(string label, string headline = null)
    {
        if (QuestionScreen.participantData != null)
        {
            float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
            float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

            string fullLabel = label;
            if (!string.IsNullOrEmpty(headline))
            {
                fullLabel += $" | Headline: {headline}";
            }

            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"{fullLabel} (local: {localTimestamp:F2}s)"
            });

            Debug.Log($"[ArticleViewerScene] Event Logged: {fullLabel} | Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
        }
        else
        {
            Debug.LogWarning("[ArticleViewerScene] Could not log event: participantData is null.");
        }
    }
}