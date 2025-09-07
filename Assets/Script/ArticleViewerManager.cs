using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ArticleViewerManager : MonoBehaviour
{
    [Header("Article Content UI")]
    public GameObject contentScrollView;

    [Header("UI References")]
    public TextMeshProUGUI topicText;
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI agreementPromptText;

    [Header("Rest Break UI")]
    public GameObject restBreakPanel;
    public TextMeshProUGUI restBreakText;

    [Header("Attention Check UI")]
    public TextMeshProUGUI attentionCheckText;

    private float articleStartTime;
    private float articleElapsedTime = 0f;
    // private const float minReadTime = 30f;
    // This is 5 minutes max reading time
    private const float maxReadTime = 300f;
    // private bool minTimeReached = false;
    private bool hasRespondedAgreement = false;
    private bool isRestBreakActive = false;
    private float restBreakStartTime = 0f;
    private float lastActionTime;
    private bool hasCompletedMinimumReadings = false;

    private SelectedArticle currentArticle;

    // Attention check fields
    private bool isAttentionCheckActive = false;
    private float attentionCheckStartTime;
    private Coroutine attentionCheckTimeout;

    private bool hasShownFinalRestBreak = false;
    private int articlesReadSinceFinalRestBreak = 0;

    private bool isInFinalRestBreak = false;

    private Coroutine showAgreementPromptCoroutine;

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
        lastActionTime = articleStartTime;
        LoadCurrentArticle();
    }

    void Update()
    {
        if (isRestBreakActive) return;

        articleElapsedTime = Time.realtimeSinceStartup - articleStartTime;

        //if (!minTimeReached && articleElapsedTime >= minReadTime)
        //{
        //    minTimeReached = true;
        //    Debug.Log("[ArticleViewerScene]: Minimum reading time reached. Participant can now proceed.");
        //}

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
        inputActions.UI.Yes.performed += OnYesPressed;
        inputActions.UI.No.performed += OnNoPressed;
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
        inputActions.UI.Yes.performed -= OnYesPressed;
        inputActions.UI.No.performed -= OnNoPressed;
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

    public void LoadArticle(SelectedArticle article)
    {
        currentArticle = article;
        headlineText.text = article.headline;
        contentText.text = article.content;
        attentionCheckText.gameObject.SetActive(false);
    }

    public void StartAttentionCheck()
    {
        if (currentArticle == null)
        {
            Debug.LogWarning("[AttentionCheck] currentArticle is null. Cannot start attention check.");
            return;
        }

        if (string.IsNullOrEmpty(currentArticle.attentionWord))
        {
            Debug.LogWarning($"[AttentionCheck] attentionWord is null or empty for article: {currentArticle.headline}. Using default.");
            currentArticle.attentionWord = "IMPORTANT"; // Default word
        }

        isAttentionCheckActive = true;
        attentionCheckStartTime = Time.time;

        // Hide agreement prompt
        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        //// Hide article headline and content
        //if (headlineText != null) headlineText.gameObject.SetActive(false);
        //if (contentText != null) contentText.gameObject.SetActive(false);
        if (contentScrollView != null) contentScrollView.SetActive(false);

        // Show attention check text
        attentionCheckText.text =
            $"Please answer this question about what you just read:\nDid the article mention the word: '{currentArticle.attentionWord}'?\n\nPress Y for YES, N for NO";
        attentionCheckText.gameObject.SetActive(true);

        Debug.Log($"[AttentionCheck] Started for article: {currentArticle.headline}, word: {currentArticle.attentionWord}");

        // Stop any previous coroutine
        if (attentionCheckTimeout != null)
            StopCoroutine(attentionCheckTimeout);

        attentionCheckTimeout = StartCoroutine(AttentionCheckTimeoutCoroutine());
    }

    private void OnYesPressed(InputAction.CallbackContext context)
    {
        if (isAttentionCheckActive)
            HandleAttentionResponse("YES");
    }

    private void OnNoPressed(InputAction.CallbackContext context)
    {
        if (isAttentionCheckActive)
            HandleAttentionResponse("NO");
    }

    private void HandleAttentionResponse(string response)
    {
        if (!isAttentionCheckActive) return;

        isAttentionCheckActive = false;

        float reactionTime = Time.time - attentionCheckStartTime;
        currentArticle.attentionCheckResponse = response;
        currentArticle.attentionCheckReactionTime = reactionTime.ToString("F3");

        Debug.Log($"[AttentionCheck] Response: {response} | local: {reactionTime:F3}s");

        if (attentionCheckTimeout != null)
        {
            StopCoroutine(attentionCheckTimeout);
            attentionCheckTimeout = null;
        }

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker == null) return;

        // Determine if final rest break should start
        bool minimumReadingsCompleted = tracker.HasReadMinimumPerFiveTopics() && tracker.GetTotalUniqueArticlesRead() >= 10;

        if (!hasShownFinalRestBreak && minimumReadingsCompleted)
        {
            hasCompletedMinimumReadings = true;
            hasShownFinalRestBreak = true;
            LogEvent("[ArticleViewer]: Minimum readings completed. Triggering final rest break.", null, lastActionTime);
            StartFinalRestBreak();
            return; // Only final rest break can interrupt here
        }

        // Hide attention check UI
        attentionCheckText.gameObject.SetActive(false);

        // --- Normal rest break logic ---
        if (!hasShownFinalRestBreak)
        {
            // Count total articles read
            int totalUniqueArticles = tracker.selectedArticles.articles.Count;

            // Show normal rest break every 10 articles
            if (totalUniqueArticles % 10 == 0)
            {
                StartRestBreak();
                return; // Prevent automatic return to TopicSelectorScene during normal rest break
            }
        }
        else
        {
            // After final rest break, track articles read for subsequent breaks
            articlesReadSinceFinalRestBreak++;

            if (articlesReadSinceFinalRestBreak % 10 == 0)
            {
                StartRestBreak();
                articlesReadSinceFinalRestBreak = 0;
                return; // Prevent automatic return to TopicSelectorScene
            }
        }

        // --- Transition back to TopicSelectorScene if no rest break ---
        LogEvent("[ArticleViewer]: Attention check completed, returning to TopicSelectorScene", currentArticle.headline, lastActionTime);
        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private IEnumerator AttentionCheckTimeoutCoroutine()
    {
        yield return new WaitForSeconds(10f);

        if (isAttentionCheckActive)
        {
            isAttentionCheckActive = false;
            currentArticle.attentionCheckResponse = "NO_RESPONSE";
            currentArticle.attentionCheckReactionTime = "TIMEOUT";

            Debug.Log("[AttentionCheck] FAILED – Timeout (10s, no response)");

            attentionCheckText.gameObject.SetActive(false);

            // --- Transition back to TopicSelectorScene ---
            LogEvent("[ArticleViewer]: Attention check completed, returning to TopicSelectorScene", currentArticle.headline, lastActionTime);
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        }
    }

    private void OnAgreementKeyPressed(string option)
    {
        if (hasRespondedAgreement) return;

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker == null || tracker.selectedArticles.articles.Count == 0)
        {
            Debug.LogWarning("[ArticleViewer]: No articles in tracker. Cannot record agreement.");
            return;
        }

        // Get the last selected article from tracker
        SelectedArticle lastArticle = tracker.selectedArticles.articles[^1];
        lastArticle.selectedOption = option;
        hasRespondedAgreement = true;

        float prevActionTime = lastActionTime;
        LogEvent($"[ArticleViewer]: AgreementSelected: {option}", lastArticle.headline, prevActionTime);

        // Assign currentArticle for attention check
        currentArticle = lastArticle;

        // Trigger the attention check
        StartAttentionCheck();

        // Update last action time
        lastActionTime = Time.realtimeSinceStartup;
    }

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        // Handle final rest break LEFT arrow
        if (isRestBreakActive && isInFinalRestBreak)
        {
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // Add the rest duration to global experiment timer now
            ExperimentTimer.Instance.AddToExperimentTime(restDuration);

            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            isInFinalRestBreak = false;
            restBreakPanel?.SetActive(false);

            LogEvent("FinalRestBreakEnded_LeftArrow", null, restBreakStartTime);

            lastActionTime = Time.realtimeSinceStartup;

            // Return to TopicSelectorScene to read more articles
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        // Normal behavior for going back
        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before going back.");
            return;
        }
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        // Handle final rest break RIGHT arrow
        if (isRestBreakActive && isInFinalRestBreak)
        {
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // Add the rest duration to global experiment timer now
            ExperimentTimer.Instance.AddToExperimentTime(restDuration);

            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            isInFinalRestBreak = false;
            restBreakPanel?.SetActive(false);

            LogEvent("FinalRestBreakEnded_RightArrow", null, restBreakStartTime);

            lastActionTime = Time.realtimeSinceStartup;

            // Proceed to end of experiment (SurveyScene)
            PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        // Normal behavior for forward arrow
        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before proceeding.");
            return;
        }
    }

    private void OnContinueRestBreak(InputAction.CallbackContext ctx)
    {
        if (!isRestBreakActive) return;

        // Only handle normal rest breaks here (not final rest break)
        if (!isInFinalRestBreak)
        {
            // Calculate how long the participant was resting
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // Add this rest duration to the global experiment timer
            ExperimentTimer.Instance.AddToExperimentTime(restDuration);

            // Adjust local timers so the article reading resumes correctly
            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            restBreakPanel?.SetActive(false);

            LogEvent("RestBreakEnded", null, restBreakStartTime);

            lastActionTime = Time.realtimeSinceStartup;

            // Go back to TopicSelectorScene
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        }
    }

    private void ShowTemporaryPromptMessage(string message)
    {
        if (agreementPromptText != null && agreementPromptText.gameObject.activeSelf)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTemporaryPrompt(agreementPromptText, message));
        }
    }

    private IEnumerator<WaitForSeconds> ShowTemporaryPrompt(TextMeshProUGUI prompt, string warningText)
    {
        string originalText = prompt.text;
        prompt.text = warningText;
        float displayDuration = warningText.Contains("completed the required readings") ? 30f : 3f;
        yield return new WaitForSeconds(displayDuration);

        if (hasCompletedMinimumReadings && warningText.Contains("completed the required readings"))
        {
            prompt.text = "";
        }
        else
        {
            prompt.text = originalText;
        }
    }

    private void LoadCurrentArticle()
    {
        var tracker = ArticleSelectionTracker.Instance;
        hasRespondedAgreement = false;

        // Stop any existing coroutine
        if (showAgreementPromptCoroutine != null)
        {
            StopCoroutine(showAgreementPromptCoroutine);
            showAgreementPromptCoroutine = null;
        }

        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[^1];
            topicText.text = lastArticle.topic;
            headlineText.text = lastArticle.headline;
            contentText.text = lastArticle.content;

            //if (agreementPromptText != null)
            //{
            //    agreementPromptText.text = "To what extent does the article align with your pre-existing beliefs or expectations?\n" +
            //                               "1 - Strong Misalignment\n2 - Misalignment\n3 - Neutral\n4 - Alignment\n5 - Strong Alignment";
            //    agreementPromptText.gameObject.SetActive(true);
            //}

            // Hide initially
            if (agreementPromptText != null)
                agreementPromptText.gameObject.SetActive(false);

            // Start coroutine to show after 10 seconds
            showAgreementPromptCoroutine = StartCoroutine(ShowAgreementPromptAfterDelay(10f));
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

    private IEnumerator ShowAgreementPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!hasRespondedAgreement && agreementPromptText != null)
        {
            agreementPromptText.text = "To what extent does the article align with your pre-existing beliefs or expectations?\n" +
                                       "1 - Strong Misalignment\n2 - Misalignment\n3 - Neutral\n4 - Alignment\n5 - Strong Alignment";
            agreementPromptText.gameObject.SetActive(true);
        }
    }

    private void StartRestBreak()
    {
        isRestBreakActive = true;
        restBreakStartTime = Time.realtimeSinceStartup;

        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (attentionCheckText != null) // Hide attention check explicitly
            attentionCheckText.gameObject.SetActive(false);

        if (restBreakPanel != null)
        {
            restBreakPanel.SetActive(true);
            if (restBreakText != null)
                restBreakText.text = $"Rest Break!\nPress SPACE to continue.";
        }

        LogEvent("RestBreakStarted", null, restBreakStartTime);
    }

    private void StartFinalRestBreak()
    {
        isRestBreakActive = true;
        isInFinalRestBreak = true;
        restBreakStartTime = Time.realtimeSinceStartup;

        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (attentionCheckText != null) // Hide attention check explicitly
            attentionCheckText.gameObject.SetActive(false);

        if (restBreakPanel != null)
        {
            restBreakPanel.SetActive(true);
            if (restBreakText != null)
                restBreakText.text = $"You have completed the required readings.\nPress LEFT to read more, or RIGHT to end the experiment.";
        }

        LogEvent("FinalRestBreakStarted", null, restBreakStartTime);
    }

    private void AutoProceed()
    {
        if (isRestBreakActive) return;

        if (!hasRespondedAgreement && !hasCompletedMinimumReadings)
        {
            OnAgreementKeyPressed("NR");
        }
    }

    private void LogEvent(string label, string headline = null, float timestampReference = 0f)
    {
        if (QuestionScreen.participantData == null) return;

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