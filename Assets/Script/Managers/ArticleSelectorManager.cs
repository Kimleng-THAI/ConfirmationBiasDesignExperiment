using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ArticleEntryData
{
    public string headline;
    public string summary;
    public string content;
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
    public Button backButton;
    public TextMeshProUGUI topicTitleText;

    private List<ArticleEntryData> loadedArticles;
    private string selectedTopic;

    // Timestamp for current scene
    private float sceneStartTime;

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        // Back button functionality
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Retrieve selected topic
        selectedTopic = PlayerPrefs.GetString("SelectedTopic", "");

        // Update UI text
        if (topicTitleText != null)
        {
            topicTitleText.text = "Selected Topic: " + selectedTopic;
        }

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

    void OnBackButtonClicked()
    {
        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = $"[ArticleSelector]: BACK_BUTTON_CLICKED (local: {localTimestamp:F2}s)"
        });

        Debug.Log($"[ArticleSelectorScene]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: BACK_BUTTON_CLICKED");

        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    string GetJsonFileNameForTopic(string topic)
    {
        Dictionary<string, string> topicToFileMap = new Dictionary<string, string>
        {
            { "Climate Change and Environmental Policy", "climate_change" },
            { "Economic Policy and Inequality", "economic_policy" },
            { "Education and Learning Methods", "education" },
            { "Health and Medical Approaches", "health" },
            { "Technology and Social Media Impact", "technology" }
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
            loadedArticles = data.articles;

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

        string label = $"[ArticleSelector]: READ_ARTICLE_BUTTON_CLICKED: {selectedArticle.headline} (local: {localTimestamp:F2}s)";

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = label
        });

        Debug.Log($"[ArticleSelectorScene]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: {label}");
        Debug.Log($"[ArticleSelectorManager]: Participant is reading article: '{selectedArticle.headline}' from topic: '{selectedTopic}'");

        // Use the tracker singleton to save the selected article
        ArticleSelectionTracker.Instance.AddSelectedArticle(
            selectedTopic,
            selectedArticle.headline,
            selectedArticle.content
        );

        // Go to ArticleViewerScene
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleViewerScene");
        SceneManager.LoadScene("TransitionScene");
    }
}