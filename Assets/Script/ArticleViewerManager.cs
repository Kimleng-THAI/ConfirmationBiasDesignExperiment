using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ArticleViewerManager : MonoBehaviour
{
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

        // Hide article headline and content
        if (headlineText != null) headlineText.gameObject.SetActive(false);
        if (contentText != null) contentText.gameObject.SetActive(false);

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

        // Final rest break logic
        if (!hasShownFinalRestBreak && tracker.HasReadMinimumTwoArticlesPerTopic(requiredTopics))
        {
            hasCompletedMinimumReadings = true;
            hasShownFinalRestBreak = true;
            LogEvent("[ArticleViewer]: All required readings completed. Triggering final rest break.", null, lastActionTime);
            StartFinalRestBreak();
            return;
        }

        attentionCheckText.gameObject.SetActive(false);

        // Restore article headline and content
        if (headlineText != null) headlineText.gameObject.SetActive(true);
        if (contentText != null) contentText.gameObject.SetActive(true);

        // Restore agreement prompt
        if (agreementPromptText != null)
            agreementPromptText.gameObject.SetActive(true);

        // Increment post-final rest break article count if final rest break already shown
        if (hasShownFinalRestBreak)
        {
            articlesReadSinceFinalRestBreak++;

            // Every 10 articles after final rest break
            if (articlesReadSinceFinalRestBreak % 10 == 0)
            {
                StartRestBreak();
                // Reset counter
                articlesReadSinceFinalRestBreak = 0;
            }
        }

        else
        {
            // Regular rest break every 10 articles before final rest break
            int totalUniqueArticles = tracker.selectedArticles.articles.Count;
            if (totalUniqueArticles % 10 == 0)
            {
                StartRestBreak();
            }
        }
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

            // Restore agreement prompt
            if (agreementPromptText != null)
                agreementPromptText.gameObject.SetActive(true);
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
        if (isRestBreakActive || isInFinalRestBreak) return;

        if (!hasRespondedAgreement && !hasCompletedMinimumReadings)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before going back.");
            return;
        }

        //if (!minTimeReached && !hasCompletedMinimumReadings)
        //{
        //    ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before going back.");
        //    return;
        //}

        var tracker = ArticleSelectionTracker.Instance;

        if (hasCompletedMinimumReadings)
        {
            LogEvent("BackButtonClicked - Experiment Completed, Returning to TopicSelectorScene", null, lastActionTime);
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        if (hasShownFinalRestBreak)
        {
            LogEvent("BackButtonClicked - Redirect to TopicSelectorScene (Final Rest Break Shown)", null, lastActionTime);
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            SceneManager.LoadScene("TransitionScene");
            return;
        }

        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            LogEvent("BackButtonClicked - Redirect to TopicSelectorScene", null, lastActionTime);
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        }
        else
        {
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        }

        SceneManager.LoadScene("TransitionScene");
    }

    private void OnForwardKeyPressed(InputAction.CallbackContext ctx)
    {
        if (isRestBreakActive || isInFinalRestBreak) return;

        if (!hasRespondedAgreement && !hasCompletedMinimumReadings)
        {
            ShowTemporaryPromptMessage("Please select your level of agreement before proceeding.");
            return;
        }

        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null)
        {
            if (hasCompletedMinimumReadings)
            {
                LogEvent("ContinueButtonClicked - Experiment Complete", null, lastActionTime);
                lastActionTime = Time.realtimeSinceStartup;
                PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
                SceneManager.LoadScene("TransitionScene");
                return;
            }

            //if (!minTimeReached)
            //{
            //    ShowTemporaryPromptMessage("Please read the article for at least 30 seconds before continuing.");
            //    return;
            //}
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

        LogEvent("RestBreakEnded", null, restBreakStartTime);

        lastActionTime = Time.realtimeSinceStartup;

        if (isInFinalRestBreak)
        {
            // After final rest break → return to ArticleViewerScene, unlock arrows
            isInFinalRestBreak = false;
            agreementPromptText?.gameObject.SetActive(true);

            if (attentionCheckText != null) // Hide just in case
                attentionCheckText.gameObject.SetActive(false);

            // stay in ArticleViewerScene
            return;
        }
        else
        {
            // Normal rest break: go back to TopicSelector
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
                restBreakText.text = $"You have completed the required readings.\nPress SPACE first before you can do either of these two actions: LEFT to read more, or RIGHT to end the experiment.";
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