using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using LSL; // ADD THIS

[System.Serializable]
public class ArticleEntryData
{
    public string headline;
    public string summary;
    public string content;
    public string attentionWord;
    public string attentionAnswer;
    // ADD THESE FIELDS FOR BIAS CALCULATION:
    public string articleCode;  // e.g., "T01A"
    public string linkedStatementCode;  // e.g., "T01-S01"
    public string articleType;  // "confirmatory", "disconfirmatory", "neutral"
}

[System.Serializable]
public class ArticleData
{
    public string topic;
    public List<ArticleEntryData> articles;
}

public class ArticleSelectorManager : MonoBehaviour
{
    // Assign in Inspector
    public GameObject articleButtonPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI topicTitleText;

    private List<ArticleEntryData> loadedArticles;
    private string selectedTopic;

    // Timestamp for current scene
    private float sceneStartTime;

    // Input actions for back key detection
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        // Retrieve selected topic
        selectedTopic = PlayerPrefs.GetString("SelectedTopic", "");

        // Update UI text
        if (topicTitleText != null)
        {
            topicTitleText.text = "Selected Topic: " + selectedTopic;
        }

        // LSL: Send article selector scene start marker
        LSLManager.Instance.SendMarker($"ARTICLE_SELECTOR_START_{GetTopicCode(selectedTopic)}");

        string jsonFileName = GetJsonFileNameForTopic(selectedTopic);

        if (!string.IsNullOrEmpty(jsonFileName))
        {
            LoadArticles(jsonFileName);
        }
        else
        {
            Debug.Log($"[ArticleSelectorManager]: No article JSON file defined for topic: {selectedTopic}");
        }
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.GoBack.performed += OnBackKeyPressed;
    }

    void OnDisable()
    {
        inputActions.UI.GoBack.performed -= OnBackKeyPressed;
        inputActions.UI.Disable();
    }

    private void OnBackKeyPressed(InputAction.CallbackContext ctx)
    {
        OnBackButtonClicked();
    }

    void OnBackButtonClicked()
    {
        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        // LSL: Send back button marker
        LSLManager.Instance.SendMarker("ARTICLE_SELECTOR_BACK");

        // Keep existing JSON event marker
        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = $"[ArticleSelector]: BACK_BUTTON_CLICKED (local: {localTimestamp:F2}s)"
        });

        Debug.Log($"[ArticleSelectorScene]: Event marker logged – Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: BACK_BUTTON_CLICKED");

        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    string GetTopicCode(string topicName)
    {
        // Map topic names to codes (matching your JSON structure)
        Dictionary<string, string> topicCodes = new Dictionary<string, string>
        {
            { "Climate Change and Environmental Policy", "T01" },
            { "Technology and Social Media Impact", "T02" },
            { "Economic Policy and Inequality", "T03" },
            { "Health and Medical Approaches", "T04" },
            { "Education and Learning Methods", "T05" },
            { "Artificial Intelligence and Ethics", "T06" },
            { "Work-Life Balance and Productivity", "T07" },
            { "Media and Information", "T15" },
            { "Science and Research Funding", "T16" },
            { "Parenting and Child Development", "T17" },
            { "Aging and Elder Care", "T18" },
            { "Mental Health and Wellness", "T20" }
        };

        return topicCodes.ContainsKey(topicName) ? topicCodes[topicName] : "T00";
    }

    string GetJsonFileNameForTopic(string topic)
    {
        Dictionary<string, string> topicToFileMap = new Dictionary<string, string>
        {
            { "Climate Change and Environmental Policy", "climate_change" },
            { "Technology and Social Media Impact", "technology" },
            { "Economic Policy and Inequality", "economic_policy" },
            { "Health and Medical Approaches", "health" },
            { "Education and Learning Methods", "education" },
            { "Artificial Intelligence and Ethics", "ai_and_ethics" },
            { "Work-Life Balance and Productivity", "work" },
            { "Media and Information", "media" },
            { "Science and Research Funding", "science" },
            { "Parenting and Child Development", "parenting" },
            { "Aging and Elder Care", "aging" },
            { "Mental Health and Wellness", "mental" }
        };

        if (topicToFileMap.ContainsKey(topic))
        {
            return topicToFileMap[topic];
        }

        return null;
    }

    void LoadArticles(string resourceFileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Articles/{resourceFileName}");

        if (jsonFile != null)
        {
            ArticleData data = JsonUtility.FromJson<ArticleData>(jsonFile.text);

            // Filter out articles already read by participant
            List<ArticleEntryData> allArticles = data.articles;
            List<ArticleEntryData> unreadArticles = new List<ArticleEntryData>();

            // Get list of read article headlines for current topic from tracker
            var tracker = ArticleSelectionTracker.Instance;
            List<string> readHeadlines = new List<string>();

            if (tracker != null)
            {
                foreach (var article in tracker.selectedArticles.articles)
                {
                    if (article.topic == selectedTopic)
                    {
                        readHeadlines.Add(article.headline);
                    }
                }
            }

            // Filter articles by checking if headline is not in readHeadlines
            foreach (var article in allArticles)
            {
                if (!readHeadlines.Contains(article.headline))
                {
                    unreadArticles.Add(article);
                }
            }

            // SHUFFLE unread articles
            for (int i = 0; i < unreadArticles.Count; i++)
            {
                int randIndex = Random.Range(i, unreadArticles.Count);
                var temp = unreadArticles[i];
                unreadArticles[i] = unreadArticles[randIndex];
                unreadArticles[randIndex] = temp;
            }

            loadedArticles = unreadArticles;

            // Clear any existing buttons
            foreach (Transform child in contentPanel)
            {
                Destroy(child.gameObject);
            }

            // Create buttons only for unread articles
            for (int i = 0; i < loadedArticles.Count; i++)
            {
                int index = i;
                GameObject btn = Instantiate(articleButtonPrefab, contentPanel);

                TextMeshProUGUI[] texts = btn.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = loadedArticles[i].headline;
                texts[1].text = loadedArticles[i].summary;

                Button readButton = btn.GetComponentInChildren<Button>();
                readButton.onClick.AddListener(() => OnReadArticleClicked(index));
            }

            // if no unread articles left, log a message
            if (loadedArticles.Count == 0)
            {
                Debug.Log("[ArticleSelectorManager]: All articles in this topic have been read.");
            }
        }
        else
        {
            Debug.LogError($"[ArticleSelectorManager]: Failed to load JSON file: {resourceFileName}.json");
        }
    }

    void OnReadArticleClicked(int index)
    {
        // Prepare selected article object
        ArticleEntryData selectedArticle = loadedArticles[index];

        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        // Get article code (ensure it's populated in your JSON)
        string articleCode = selectedArticle.articleCode ?? $"{GetTopicCode(selectedTopic)}A{index}";

        // LSL: Send article preview marker
        LSLManager.Instance.SendMarker($"ARTICLE_PREVIEW_{articleCode}");

        // Calculate EXPECTED bias response
        if (!string.IsNullOrEmpty(selectedArticle.linkedStatementCode))
        {
            var expectedBias = ExpectedVsActualBiasSystem.Instance.CalculateExpectedResponse(articleCode);

            if (expectedBias != null)
            {
                LSLManager.Instance.SendMarker($"EXPECTED_BIAS_{articleCode}_{expectedBias.expectedResponse}");
                Debug.Log($"[ArticleSelector]: Expected bias for {articleCode}: {expectedBias.expectedResponse}");
            }
            else
            {
                Debug.LogWarning($"[ArticleSelector]: Expected bias calculation returned null for {articleCode}");
            }
        }

        // LSL: Send article selection marker
        LSLManager.Instance.SendMarker($"ARTICLE_SELECT_{articleCode}");

        // Keep existing JSON event marker
        string label = $"[ArticleSelector]: READ_ARTICLE_BUTTON_CLICKED: {selectedArticle.headline} (local: {localTimestamp:F2}s)";

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = label
        });

        Debug.Log($"[ArticleSelectorScene]: Event marker logged – Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: {label}");
        Debug.Log($"[ArticleSelectorManager]: Participant is reading article: '{selectedArticle.headline}' from topic: '{selectedTopic}'");

        // Use the tracker singleton to save the selected article
        ArticleSelectionTracker.Instance.AddSelectedArticle(
            selectedTopic,
            selectedArticle.headline,
            selectedArticle.content,
            selectedArticle.attentionWord,
            selectedArticle.attentionAnswer,
            selectedArticle.articleCode,
            selectedArticle.linkedStatementCode
        );

        // Store article code for Phase 2 processing
        PlayerPrefs.SetString("CurrentArticleCode", articleCode);
        PlayerPrefs.Save();

        // Go to ArticleViewerScene
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleViewerScene");
        SceneManager.LoadScene("TransitionScene");
    }
}