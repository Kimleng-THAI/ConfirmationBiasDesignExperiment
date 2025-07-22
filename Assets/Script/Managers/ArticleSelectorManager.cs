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

        if (selectedTopic == "Climate Change and Environmental Policy")
        {
            LoadArticles("climate_change");
        }
        else
        {
            Debug.Log($"[ArticleSelectorManager]: No article data loaded for topic: {selectedTopic}");
            // Optional: show a message like "Articles for this topic are coming soon!"
        }
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("TopicSelectorScene");
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
        PlayerPrefs.SetString("SelectedTopic", "climate_change");
        SceneManager.LoadScene("ArticleViewerScene");
    }
}