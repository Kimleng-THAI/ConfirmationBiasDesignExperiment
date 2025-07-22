using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArticleViewerManager : MonoBehaviour
{
    public TextMeshProUGUI headlineText;
    public TextMeshProUGUI contentText;
    public Button backButton;
    public Button continueButton;

    void Start()
    {
        backButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("ArticleSelectorScene");
        });

        continueButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("SurveyScene");
            Debug.Log("[ArticleViewerScene]: Participant has finished reading the article.");
        });

        // Load full list
        string json = PlayerPrefs.GetString("SelectedArticles", "");
        if (!string.IsNullOrEmpty(json))
        {
            SelectedArticleList articleList = JsonUtility.FromJson<SelectedArticleList>(json);
            if (articleList.articles.Count > 0)
            {
                SelectedArticle lastArticle = articleList.articles[articleList.articles.Count - 1];
                headlineText.text = lastArticle.headline;
                contentText.text = lastArticle.content;
            }
            else
            {
                headlineText.text = "No Article Selected";
                contentText.text = "Please go back and choose an article.";
            }
        }
        else
        {
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
        }
    }
}