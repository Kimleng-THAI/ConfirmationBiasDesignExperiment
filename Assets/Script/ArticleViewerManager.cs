// ArticleViewerManager.cs - Updated for Real-time LSL Streaming
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
    private const float maxReadTime = 300f; // 5 minutes max reading time
    private bool hasRespondedAgreement = false;
    private bool isRestBreakActive = false;
    private float restBreakStartTime = 0f;
    private float lastActionTime;
    private bool hasCompletedMinimumReadings = false;

    private SelectedArticle currentArticle;
    private string currentArticleCode;
    private string currentTopicCode;

    // Attention check fields
    private bool isAttentionCheckActive = false;
    private float attentionCheckStartTime;
    private Coroutine attentionCheckTimeout;

    private bool hasShownFinalRestBreak = false;
    private int articlesReadSinceFinalRestBreak = 0;
    private bool isInFinalRestBreak = false;

    private Coroutine showAgreementPromptCoroutine;

    // Scroll tracking
    private float lastScrollPosition = 0f;
    private float maxScrollReached = 0f;
    private Coroutine scrollTrackingCoroutine;
    private int scrollEventCount = 0;

    // Reading behavior tracking
    private float articleAgreementTime = 0f;
    private float totalDwellTime = 0f;

    private List<string> requiredTopics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Technology and Social Media Impact",
        "Economic Policy and Inequality",
        "Health and Medical Approaches",
        "Education and Learning Methods",
        "Artificial Intelligence and Ethics",
        "Work-Life Balance and Productivity",
        "Media and Information",
        "Science and Research Funding",
        "Parenting and Child Development",
        "Aging and Elder Care",
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

        // Get article code from PlayerPrefs
        currentArticleCode = PlayerPrefs.GetString("CurrentArticleCode", "");

        // Extract topic code from article code (e.g., T01A -> T01)
        if (currentArticleCode.Length >= 3)
        {
            currentTopicCode = currentArticleCode.Substring(0, 3);
        }

        // === REAL-TIME LSL: Send article read start ===
        LSLManager.Instance.SendMarker($"ARTICLE_READ_START_{currentArticleCode}");

        // Send behavioral event for article start
        var startData = new Dictionary<string, object>
        {
            ["articleCode"] = currentArticleCode,
            ["topicCode"] = currentTopicCode
        };
        LSLManager.Instance.SendBehavioralEvent("ArticleReadStart", startData);

        LoadCurrentArticle();

        // Start scroll tracking
        scrollTrackingCoroutine = StartCoroutine(TrackScrolling());
    }

    void Update()
    {
        if (isRestBreakActive) return;

        articleElapsedTime = Time.realtimeSinceStartup - articleStartTime;
        totalDwellTime = articleElapsedTime;

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

        if (scrollTrackingCoroutine != null)
            StopCoroutine(scrollTrackingCoroutine);
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

    private IEnumerator TrackScrolling()
    {
        ScrollRect scrollRect = contentScrollView?.GetComponent<ScrollRect>();
        if (scrollRect == null) yield break;

        while (true)
        {
            float currentScroll = scrollRect.verticalNormalizedPosition;

            // Track max scroll depth
            if (currentScroll < maxScrollReached) // Lower values = scrolled down more
            {
                maxScrollReached = currentScroll;
            }

            // Send scroll event if significant change
            if (Mathf.Abs(currentScroll - lastScrollPosition) > 0.1f)
            {
                scrollEventCount++;
                float scrollDepth = 1 - currentScroll; // Convert to 0-1 where 1 is fully scrolled

                // === REAL-TIME LSL: Send scroll behavior ===
                if (scrollEventCount % 5 == 0) // Send every 5th scroll event to reduce noise
                {
                    var scrollData = new Dictionary<string, object>
                    {
                        ["articleCode"] = currentArticleCode,
                        ["scrollDepth"] = scrollDepth,
                        ["timeInArticle"] = Time.realtimeSinceStartup - articleStartTime
                    };
                    LSLManager.Instance.SendBehavioralEvent("ArticleScroll", scrollData);
                }

                lastScrollPosition = currentScroll;
            }

            yield return new WaitForSeconds(0.5f);
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

        float readingTime = Time.realtimeSinceStartup - articleStartTime;
        articleAgreementTime = readingTime;

        // === REAL-TIME LSL STREAMING ===
        // Send article response immediately
        LSLManager.Instance.SendArticleResponse(
            currentArticleCode,
            currentTopicCode,
            int.Parse(option),
            readingTime,
            1 - maxScrollReached  // Scroll depth
        );

        // Send marker
        LSLManager.Instance.SendMarker($"ARTICLE_RATING_{currentArticleCode}_R{option}_TIME{readingTime:F1}");

        // Send detailed behavioral data
        LSLManager.Instance.SendArticleReadingBehavior(
            currentArticleCode,
            readingTime,
            1 - maxScrollReached,
            0  // backButtonCount - track if needed
        );

        // === JSON BACKUP (keep existing system) ===
        // Record ACTUAL response for bias comparison
        ExpectedVsActualBiasSystem.Instance.RecordActualResponse(
            currentArticleCode,
            int.Parse(option),
            readingTime,
            1 - maxScrollReached
        );

        LogEvent($"[ArticleViewer]: AgreementSelected: {option}", lastArticle.headline, lastActionTime);

        // Assign currentArticle for attention check
        currentArticle = lastArticle;

        // Trigger the attention check
        StartAttentionCheck();

        // Update last action time
        lastActionTime = Time.realtimeSinceStartup;
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
            currentArticle.attentionWord = "IMPORTANT";
        }

        isAttentionCheckActive = true;
        attentionCheckStartTime = Time.time;

        // Hide agreement prompt and content
        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);
        if (contentScrollView != null)
            contentScrollView.SetActive(false);

        // Show attention check text
        attentionCheckText.text =
            $"Please answer this question about what you just read:\nDid the article mention the word: '{currentArticle.attentionWord}'?\n\nPress Right Arrow Key for YES, Left Arrow Key for NO";
        attentionCheckText.gameObject.SetActive(true);

        // === REAL-TIME LSL ===
        LSLManager.Instance.SendMarker($"ATTENTION_CHECK_START_PHASE2_{currentArticleCode}");

        Debug.Log($"[AttentionCheck] Started for article: {currentArticle.headline}, word: {currentArticle.attentionWord}");

        if (attentionCheckTimeout != null)
            StopCoroutine(attentionCheckTimeout);

        attentionCheckTimeout = StartCoroutine(AttentionCheckTimeoutCoroutine());
    }

    private void HandleAttentionResponse(string response)
    {
        if (!isAttentionCheckActive || currentArticle == null) return;

        isAttentionCheckActive = false;

        float reactionTime = Time.time - attentionCheckStartTime;
        currentArticle.attentionCheckResponse = response;
        currentArticle.attentionCheckReactionTime = reactionTime.ToString("F3");

        // Determine correctness
        string correctness = (response == currentArticle.attentionAnswer?.ToUpper()) ? "CORRECT" : "INCORRECT";

        // === REAL-TIME LSL ===
        // Update article response with attention check data
        LSLManager.Instance.SendArticleResponse(
            currentArticleCode,
            currentTopicCode,
            int.Parse(currentArticle.selectedOption ?? "0"),
            articleAgreementTime,
            1 - maxScrollReached,
            response,
            reactionTime
        );

        LSLManager.Instance.SendMarker($"ATTENTION_CHECK_RESPONSE_PHASE2_{correctness}_RT{reactionTime:F3}");

        Debug.Log($"[AttentionCheck] Response: {response} | RT: {reactionTime:F3}s");

        if (attentionCheckTimeout != null)
        {
            StopCoroutine(attentionCheckTimeout);
            attentionCheckTimeout = null;
        }

        // === Handle rest breaks and transitions ===
        var tracker = ArticleSelectionTracker.Instance;
        if (tracker == null)
        {
            Debug.LogWarning("[AttentionCheck] Tracker is null. Skipping further logic.");
            attentionCheckText.gameObject.SetActive(false);
            return;
        }

        // Determine if final rest break should start
        bool minimumReadingsCompleted = tracker.HasReadMinimumPerFiveTopics() && tracker.GetTotalUniqueArticlesRead() >= 10;

        if (!hasShownFinalRestBreak && minimumReadingsCompleted)
        {
            hasCompletedMinimumReadings = true;
            hasShownFinalRestBreak = true;
            LogEvent("[ArticleViewer]: Minimum readings completed. Triggering final rest break.", null, lastActionTime);
            StartFinalRestBreak();
            return;
        }

        // Hide attention check UI
        attentionCheckText.gameObject.SetActive(false);

        // Normal rest break logic
        if (!hasShownFinalRestBreak)
        {
            int totalUniqueArticles = tracker.selectedArticles.articles.Count;

            if (totalUniqueArticles % 10 == 0)
            {
                StartRestBreak();
                return;
            }
        }
        else
        {
            articlesReadSinceFinalRestBreak++;

            if (articlesReadSinceFinalRestBreak % 10 == 0)
            {
                StartRestBreak();
                articlesReadSinceFinalRestBreak = 0;
                return;
            }
        }

        // Transition back to TopicSelectorScene
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

            // === REAL-TIME LSL ===
            LSLManager.Instance.SendArticleResponse(
                currentArticleCode,
                currentTopicCode,
                int.Parse(currentArticle.selectedOption ?? "0"),
                articleAgreementTime,
                1 - maxScrollReached,
                "TIMEOUT",
                10f
            );

            Debug.Log("[AttentionCheck] FAILED - Timeout (10s, no response)");

            attentionCheckText.gameObject.SetActive(false);

            // Transition back
            LogEvent("[ArticleViewer]: Attention check completed, returning to TopicSelectorScene", currentArticle.headline, lastActionTime);
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        }
    }

    private void StartRestBreak()
    {
        isRestBreakActive = true;
        restBreakStartTime = Time.realtimeSinceStartup;

        // === REAL-TIME LSL ===
        LSLManager.Instance.SendMarker("REST_BREAK_START_PHASE2");

        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (attentionCheckText != null)
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

        // === REAL-TIME LSL ===
        LSLManager.Instance.SendMarker("FINAL_REST_BREAK_START");

        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(false);

        if (attentionCheckText != null)
            attentionCheckText.gameObject.SetActive(false);

        if (restBreakPanel != null)
        {
            restBreakPanel.SetActive(true);
            if (restBreakText != null)
                restBreakText.text = $"You have completed the required readings.\nPress LEFT to read more, or RIGHT to end the experiment.";
        }

        LogEvent("FinalRestBreakStarted", null, restBreakStartTime);
    }

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        if (isAttentionCheckActive)
        {
            HandleAttentionResponse("NO");
            return;
        }

        if (isRestBreakActive && isInFinalRestBreak)
        {
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // === REAL-TIME LSL ===
            LSLManager.Instance.SendRestBreak("FinalRestBreak_ContinueReading", restDuration);

            ExperimentTimer.Instance.AddToExperimentTime(restDuration);
            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            isInFinalRestBreak = false;
            restBreakPanel?.SetActive(false);

            LogEvent("FinalRestBreakEnded_LeftArrow", null, restBreakStartTime);
            lastActionTime = Time.realtimeSinceStartup;

            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before going back.");
            return;
        }
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        if (isAttentionCheckActive)
        {
            HandleAttentionResponse("YES");
            return;
        }

        if (isRestBreakActive && isInFinalRestBreak)
        {
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // === REAL-TIME LSL ===
            LSLManager.Instance.SendRestBreak("FinalRestBreak_EndExperiment", restDuration);

            ExperimentTimer.Instance.AddToExperimentTime(restDuration);
            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            isInFinalRestBreak = false;
            restBreakPanel?.SetActive(false);

            LogEvent("FinalRestBreakEnded_RightArrow", null, restBreakStartTime);
            lastActionTime = Time.realtimeSinceStartup;

            // Proceed to end of experiment
            PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        if (!hasRespondedAgreement)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before proceeding.");
            return;
        }
    }

    private void OnContinueRestBreak(InputAction.CallbackContext ctx)
    {
        if (!isRestBreakActive) return;

        if (!isInFinalRestBreak)
        {
            float restDuration = Time.realtimeSinceStartup - restBreakStartTime;

            // === REAL-TIME LSL ===
            LSLManager.Instance.SendRestBreak("NormalRestBreak", restDuration);

            ExperimentTimer.Instance.AddToExperimentTime(restDuration);
            articleStartTime += restDuration;
            lastActionTime += restDuration;

            isRestBreakActive = false;
            restBreakPanel?.SetActive(false);

            LogEvent("RestBreakEnded", null, restBreakStartTime);
            lastActionTime = Time.realtimeSinceStartup;

            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        }
    }

    // ... [Keep all other helper methods like ShowTemporaryPromptMessage, LoadCurrentArticle, etc.] ...

    private void ShowTemporaryPromptMessage(string message)
    {
        if (agreementPromptText != null && agreementPromptText.gameObject.activeSelf)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTemporaryPrompt(agreementPromptText, message));
        }
    }

    private IEnumerator ShowTemporaryPrompt(TextMeshProUGUI prompt, string warningText)
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

            if (agreementPromptText != null)
                agreementPromptText.gameObject.SetActive(false);

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
            agreementPromptText.text = "To what extent does the article align with your pre-existing beliefs or expectations?\n";
            agreementPromptText.gameObject.SetActive(true);
        }
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
        // Keep JSON backup logging
        if (QuestionScreen.participantData == null) return;

        float localTimestamp = Time.realtimeSinceStartup - timestampReference;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        string fullLabel = label;

        if (!string.IsNullOrEmpty(headline))
            fullLabel += $" | Headline: {headline}";

        if (currentArticle != null)
        {
            if (!string.IsNullOrEmpty(currentArticle.articleCode))
                fullLabel += $" | ArticleCode: {currentArticle.articleCode}";

            if (!string.IsNullOrEmpty(currentArticle.linkedStatement))
                fullLabel += $" | LinkedStatement: {currentArticle.linkedStatement}";

            if (!string.IsNullOrEmpty(currentArticle.attentionCheckResponse))
                fullLabel += $" | AttentionCheckResponse: {currentArticle.attentionCheckResponse}";
        }

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = $"{fullLabel} (local: {localTimestamp:F2}s)"
        });

        Debug.Log($"[ArticleViewerScene] Event Logged: {fullLabel} | Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
    }
}