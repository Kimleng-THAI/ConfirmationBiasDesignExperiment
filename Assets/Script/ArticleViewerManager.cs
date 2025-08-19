using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ArticleViewerManager : MonoBehaviour
{
    public TextMeshProUGUI topicText;
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI agreementPromptText;

    // UI panel displayed during rest break
    public GameObject restBreakPanel;
    public TextMeshProUGUI restBreakText;

    // Timestamp for current scene
    private float articleStartTime;      // Time when article is displayed
    private float articleElapsedTime = 0f;
    private const float minReadTime = 30f; // 30 seconds
    private const float maxReadTime = 300f; // 5 minutes
    private bool minTimeReached = false;

    private bool hasRespondedAgreement = false;

    // Rest break state
    private bool isRestBreakActive = false;
    private float restBreakStartTime = 0f;

    private float lastActionTime; // Tracks time of last meaningful participant action

    private List<string> requiredTopics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Technology and Social Media Impact",
        "Economic Policy and Inequality",
        "Health and Medical Approaches",
        "Education and Learning Methods",
        "Artificial Intelligence and Ethics",
        "Work-Life Balance and Productivity",
        "Urban Planning and Housing",
        "Food Systems and Agriculture",
        "Criminal Justice and Rehabilitation",
        "Gender and Society",
        "Immigration and Cultural Integration",
        "Privacy and Surveillance",
        "Sports and Competition",
        "Media and Information",
        "Science and Research Funding",
        "Parenting and Child Development",
        "Aging and Elder Care",
        "Transportation and Mobility",
        "Mental Health and Wellness"
    };

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        articleStartTime = Time.realtimeSinceStartup;
        lastActionTime = articleStartTime; // Initialize local timestamp reference
        LoadCurrentArticle();
    }

    void Update()
    {
        // Pause timing during rest break
        if (isRestBreakActive) return;

        articleElapsedTime = Time.realtimeSinceStartup - articleStartTime;

        if (!minTimeReached && articleElapsedTime >= minReadTime)
        {
            minTimeReached = true;
            Debug.Log("[ArticleViewerScene]: Minimum reading time reached. Participant can now proceed.");
        }

        if (articleElapsedTime >= maxReadTime)
        {
            Debug.Log("[ArticleViewerScene]: Maximum reading time reached. Automatically proceeding.");
            AutoProceed();
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
        inputActions.UI.Continue.performed += OnContinueRestBreak;
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
        inputActions.UI.Continue.performed -= OnContinueRestBreak;
        inputActions.UI.Disable();
    }

    private void OnSelect1(InputAction.CallbackContext ctx) => OnNumberKeyPressed("1");
    private void OnSelect2(InputAction.CallbackContext ctx) => OnNumberKeyPressed("2");
    private void OnSelect3(InputAction.CallbackContext ctx) => OnNumberKeyPressed("3");
    private void OnSelect4(InputAction.CallbackContext ctx) => OnNumberKeyPressed("4");
    private void OnSelect5(InputAction.CallbackContext ctx) => OnNumberKeyPressed("5");

    private void OnNumberKeyPressed(string number)
    {
        if (!hasRespondedAgreement)
        {
            OnAgreementKeyPressed(number);
        }
    }

    private void OnAgreementKeyPressed(string option)
    {
        if (hasRespondedAgreement) return;

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            lastArticle.selectedOption = option;
            hasRespondedAgreement = true;

            LogEvent($"[ArticleViewer]: AgreementSelected: {option}", lastArticle.headline, lastActionTime);
            Debug.Log($"[ArticleViewerScene]: Agreement response recorded: {option}");

            lastActionTime = Time.realtimeSinceStartup;

            // Check for rest break after response
            int totalUniqueArticles = tracker.selectedArticles.articles.Count;
            if (totalUniqueArticles % 10 == 0)
            {
                StartRestBreak();
            }
        }
    }

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        // Prevent going back until a response is given
        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before going back.");
            return;
        }

        if (!minTimeReached)
        {
            ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before going back.");
            return;
        }

        var tracker = ArticleSelectionTracker.Instance;
        string currentTopic = tracker?.selectedArticles.articles[^1].topic;

        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            int articlesReadInCurrentTopic = tracker.GetUniqueArticleCountForTopic(currentTopic);
            Debug.Log($"Articles read in current topic '{currentTopic}': {articlesReadInCurrentTopic}");

            LogEvent(
                articlesReadInCurrentTopic >= 2 ? "BackButtonClicked - Redirect to TopicSelectorScene" :
                                                  "BackButtonClicked - Redirect to ArticleSelectorScene",
                null,
                lastActionTime);

            lastActionTime = Time.realtimeSinceStartup;
            PlayerPrefs.SetString("NextSceneAfterTransition",
                articlesReadInCurrentTopic >= 2 ? "TopicSelectorScene" : "ArticleSelectorScene");
        }
        else
        {
            PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
        }

        SceneManager.LoadScene("TransitionScene");
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        if (isRestBreakActive) return;

        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before proceeding.");
            return;
        }

        if (!minTimeReached)
        {
            ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before continuing.");
            return;
        }

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

    private void OnContinueRestBreak(InputAction.CallbackContext ctx)
    {
        if (!isRestBreakActive) return;

        float restDuration = Time.realtimeSinceStartup - restBreakStartTime;
        articleStartTime += restDuration;
        lastActionTime += restDuration;
        ExperimentTimer.Instance.AddToExperimentTime(restDuration);

        isRestBreakActive = false;
        restBreakPanel?.SetActive(false);
        agreementPromptText?.gameObject.SetActive(true);

        Debug.Log("[ArticleViewerScene]: Rest break ended. Timestamps resumed.");
        // Log event with restBreakStartTime as baseline
        LogEvent("RestBreakEnded", null, restBreakStartTime);

        lastActionTime = Time.realtimeSinceStartup;

        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private void ShowTemporaryPromptMessage(string message)
    {
        if (agreementPromptText != null && agreementPromptText.gameObject.activeSelf)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTemporaryPrompt(agreementPromptText, message));
        }

        Debug.Log("[ArticleViewerScene]: " + message);
    }

    private IEnumerator<WaitForSeconds> ShowTemporaryPrompt(TextMeshProUGUI prompt, string warningText)
    {
        string originalText = prompt.text;
        prompt.text = warningText;
        yield return new WaitForSeconds(2f);
        prompt.text = originalText;
    }

    private void LoadCurrentArticle()
    {
        var tracker = ArticleSelectionTracker.Instance;
        hasRespondedAgreement = false;

        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            topicText.text = lastArticle.topic;
            headlineText.text = lastArticle.headline;
            contentText.text = lastArticle.content;

            if (agreementPromptText != null)
            {
                agreementPromptText.text = "How much do you agree or disagree with the above article?\n" +
                                           "1 - Strongly Disagree\n" +
                                           "2 - Disagree\n" +
                                           "3 - Neutral\n" +
                                           "4 - Agree\n" +
                                           "5 - Strongly Agree";
                agreementPromptText.gameObject.SetActive(true);
            }
        }
        else
        {
            topicText.text = "No Topic";
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
            if (agreementPromptText != null)
            {
                agreementPromptText.text = "";
            }
        }
    }

    private void StartRestBreak(int articlesUntilNextBreak = 10)
    {
        isRestBreakActive = true;
        restBreakStartTime = Time.realtimeSinceStartup;

        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (restBreakPanel != null)
        {
            restBreakPanel.SetActive(true);
            if (restBreakText != null)
                restBreakText.text = $"Rest Break!\nPress SPACE to continue.";
        }

        Debug.Log("[ArticleViewerScene]: Rest break started. Timestamps paused.");
        // Log event with restBreakStartTime as baseline
        LogEvent("RestBreakStarted", null, restBreakStartTime);
    }

    private void AutoProceed()
    {
        if (!hasRespondedAgreement)
        {
            Debug.Log("[ArticleViewerScene]: Participant did not respond.");
            OnAgreementKeyPressed("NR");
        }

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.HasReadMinimumTwoArticlesPerTopic(requiredTopics))
        {
            LogEvent("AutoProceed_MaxTimeReached");
            PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
            SceneManager.LoadScene("TransitionScene");
        }
        else
        {
            Debug.Log("[ArticleViewerScene]: Auto proceed blocked. Participant hasn't read 2 unique articles per topic.");
        }
    }

    private void LogEvent(string label, string headline = null, float timestampReference = 0f)
    {
        if (QuestionScreen.participantData == null)
        {
            Debug.LogWarning("[ArticleViewerScene] Could not log event: participantData is null.");
            return;
        }

        float localTimestamp = Time.realtimeSinceStartup - timestampReference;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        string fullLabel = label;
        if (!string.IsNullOrEmpty(headline))
            fullLabel += $" | Headline: {headline}";

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = $"{fullLabel} (local: {localTimestamp:F2}s)"
        });

        Debug.Log($"[ArticleViewerScene] Event Logged: {fullLabel} | Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
    }
}