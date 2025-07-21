using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    private List<ArticleEntryData> loadedArticles;

    void Start()
    {
        LoadArticles();
    }

    void LoadArticles()
    {
        // Load JSON from Resources/Articles
        TextAsset jsonFile = Resources.Load<TextAsset>("Articles/climate_change");

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
            Debug.LogError("Failed to load climate_change.json from Resources/Articles");
        }
    }

    void OnReadArticleClicked(int index)
    {
        PlayerPrefs.SetInt("SelectedArticleIndex", index);
        PlayerPrefs.SetString("SelectedTopic", "climate_change");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ArticleViewerScene");
    }
}
