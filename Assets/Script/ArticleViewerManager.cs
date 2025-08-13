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
    private float sceneStartTime;

    // Reading state
    private bool hasResponded = false; // Track if participant already gave a response
    private float articleElapsedTime = 0f;
    private const float minReadTime = 30f; // 30 seconds
    private const float maxReadTime = 300f; // 5 minutes
    private bool minTimeReached = false;

    // Rest break state
    private bool isRestBreakActive = false;
    private float restBreakStartTime = 0f;

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
        sceneStartTime = Time.realtimeSinceStartup;
        LoadCurrentArticle();
    }

    void Update()
    {
        // Pause timing during rest break
        if (isRestBreakActive) return;

        articleElapsedTime += Time.deltaTime;

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

    private void OnSelect1(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("1");
    private void OnSelect2(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("2");
    private void OnSelect3(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("3");
    private void OnSelect4(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("4");
    private void OnSelect5(InputAction.CallbackContext ctx) => OnAgreementKeyPressed("5");

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        // Prevent going back until a response is given
        if (!hasResponded)
        {
            Debug.Log("[ArticleViewerScene]: Back action ignored. Participant must select a level of agreement first.");
            ShowTemporaryPromptMessage("Please select your level of agreement before going back.");
            return;
        }

        if (!minTimeReached)
        {
            Debug.Log("[ArticleViewerScene]: Backward action ignored. Participant must read at least 30 seconds.");
            ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before going back to previous scene.");
            return;
        }

        var tracker = ArticleSelectionTracker.Instance;

        // Once participant has read 2 articles in current topic
        // Left Arrow Key goes back to TopicSelectorScene
        // Else, it goes back to ArticleSelectorScene
        string currentTopic = null;
        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            currentTopic = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1].topic;
            int articlesReadInCurrentTopic = tracker.GetUniqueArticleCountForTopic(currentTopic);
            Debug.Log($"Articles read in current topic '{currentTopic}': {articlesReadInCurrentTopic}");

            if (articlesReadInCurrentTopic >= 2)
            {
                LogEvent("BackButtonClicked - Redirect to TopicSelectorScene");
                PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            }
            else
            {
                LogEvent("BackButtonClicked - Redirect to ArticleSelectorScene");
                PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
            }
        }
        else
        {
            // Fallback if no current topic found
            PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
        }
        SceneManager.LoadScene("TransitionScene");
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        if (isRestBreakActive)
        {
            Debug.Log("[ArticleViewerScene]: Forward ignored during rest break.");
            return;
        }

        // Prevent going to SurveyScene until a response is given
        if (!hasResponded)
        {
            Debug.Log("[ArticleViewerScene]: Forward action ignored. Participant must select a level of agreement first.");
            ShowTemporaryPromptMessage("Please select your level of agreement before proceeding.");
            return;
        }

        if (!minTimeReached)
        {
            Debug.Log("[ArticleViewerScene]: Forward action ignored. Participant must read at least 30 seconds.");
            ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before continuing.");
            return;
        }

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
        if (hasResponded) return;

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            lastArticle.selectedOption = option;
            hasResponded = true;
            LogEvent($"[ArticleViewer]: AgreementSelected: {option}", lastArticle.headline);
            Debug.Log($"[ArticleViewerScene]: Agreement response recorded: {option}");

            // Check if rest break should now occur **only after response**
            int totalUniqueArticles = tracker.selectedArticles.articles.Count;
            if (totalUniqueArticles % 10 == 0)
            {
                // will hide agreementPromptText and show restBreakPanel
                StartRestBreak();
            }
        }
    }

    private void OnContinueRestBreak(InputAction.CallbackContext ctx)
    {
        if (!isRestBreakActive) return;

        float restDuration = Time.realtimeSinceStartup - restBreakStartTime;
        sceneStartTime += restDuration;
        ExperimentTimer.Instance.AddToExperimentTime(restDuration);

        isRestBreakActive = false;

        if (restBreakPanel != null)
            restBreakPanel.SetActive(false);

        // Optionally re-enable agreement prompt for next article
        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(true);

        Debug.Log("[ArticleViewerScene]: Rest break ended. Timestamps resumed.");
        LogEvent("RestBreakEnded");

        // Go to TopicSelectorScene
        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private void ShowTemporaryPromptMessage(string message)
    {
        if (agreementPromptText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTemporaryPrompt(message));
        }
        Debug.Log("[ArticleViewerScene]: " + message);
    }

    private IEnumerator<WaitForSeconds> ShowTemporaryPrompt(string warningText)
    {
        string originalText = agreementPromptText.text;
        agreementPromptText.text = warningText;
        // Show warning for 2 seconds
        yield return new WaitForSeconds(2f);
        agreementPromptText.text = originalText;
    }

    private void LoadCurrentArticle()
    {
        var tracker = ArticleSelectionTracker.Instance;

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
                agreementPromptText.gameObject.SetActive(true); // Ensure it's visible for new article
            }
        }
        else
        {
            topicText.text = "No Topic";
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
            if (agreementPromptText != null)
                agreementPromptText.text = "";
        }
    }

    private void CheckForRestBreak()
    {
        var tracker = ArticleSelectionTracker.Instance;
        int totalUniqueArticles = tracker.selectedArticles.articles.Count;

        if (totalUniqueArticles > 0 && totalUniqueArticles % 10 == 0)
        {
            int articlesUntilNextBreak = 10 - (totalUniqueArticles % 10);
            StartRestBreak(articlesUntilNextBreak);
        }
    }

    private void StartRestBreak(int articlesUntilNextBreak = 10)
    {
        isRestBreakActive = true;
        restBreakStartTime = Time.realtimeSinceStartup;

        // Hide agreement prompt
        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (restBreakPanel != null)
        {
            restBreakPanel.SetActive(true);
            if (restBreakText != null)
            {
                restBreakText.text = $"Rest Break!\nPress SPACE to continue.";
            }
        }

        Debug.Log("[ArticleViewerScene]: Rest break started. Timestamps paused.");
        LogEvent("RestBreakStarted");
    }

    private void AutoProceed()
    {
        if (!hasResponded)
        {
            Debug.Log("[ArticleViewerScene]: Participant did not respond. Logging default response '3' (Neutral).");
            OnAgreementKeyPressed("3");
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