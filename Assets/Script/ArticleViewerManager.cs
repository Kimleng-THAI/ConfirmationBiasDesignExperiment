using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArticleViewerManager : MonoBehaviour
{
    public TextMeshProUGUI topicText;
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

        // Get the most recently viewed article from the tracker
        var tracker = ArticleSelectionTracker.Instance;
        if (tracker != null && tracker.selectedArticles.articles.Count > 0)
        {
            SelectedArticle lastArticle = tracker.selectedArticles.articles[tracker.selectedArticles.articles.Count - 1];
            topicText.text = lastArticle.topic;
            headlineText.text = lastArticle.headline;
            contentText.text = lastArticle.content;
        }
        else
        {
            topicText.text = "No Topic";
            headlineText.text = "No Article Selected";
            contentText.text = "Please go back and choose an article.";
        }
    }
}