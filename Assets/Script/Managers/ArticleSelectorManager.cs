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
    private string selectedTopic;

    void Start()
    {
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
        // Prepare selected article object
        ArticleEntryData selectedArticle = loadedArticles[index];

        SelectedArticle newEntry = new SelectedArticle
        {
            topic = selectedTopic,
            headline = selectedArticle.headline,
            content = selectedArticle.content
        };

        // Load existing list or create new one
        string existingJson = PlayerPrefs.GetString("SelectedArticles", "");
        SelectedArticleList articleList;

        if (!string.IsNullOrEmpty(existingJson))
        {
            articleList = JsonUtility.FromJson<SelectedArticleList>(existingJson);
        }
        else
        {
            articleList = new SelectedArticleList();
        }

        // Add and save updated list
        articleList.articles.Add(newEntry);
        string updatedJson = JsonUtility.ToJson(articleList);
        PlayerPrefs.SetString("SelectedArticles", updatedJson);
        PlayerPrefs.Save();

        // Go to ArticleViewerScene
        SceneManager.LoadScene("ArticleViewerScene");
    }
}