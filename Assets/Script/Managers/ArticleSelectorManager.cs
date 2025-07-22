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
    public GameObject articleButtonPrefab; // Assign in Inspector
    public Transform contentPanel;         // Assign in Inspector
    public Button backButton;              // Assign in Inspector
    public TextMeshProUGUI topicTitleText; // Assign in Inspector

    private List<ArticleEntryData> loadedArticles;
    // Store selected topic at class level
    private string selectedTopic;

    void Start()
    {
        // Ensure back button always works
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Get selected topic from PlayerPrefs
        string selectedTopic = PlayerPrefs.GetString("SelectedTopic", "");

        // Update the TopicTitleText dynamically
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
        SceneManager.LoadScene("TopicSelectorScene");
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
        PlayerPrefs.SetInt("SelectedArticleIndex", index);
        PlayerPrefs.SetString("SelectedTopic", selectedTopic);
        SceneManager.LoadScene("ArticleViewerScene");
    }
}