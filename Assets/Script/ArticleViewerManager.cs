using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
            PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        });

        continueButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString("NextSceneAfterTransition", "SurveyScene");
            SceneManager.LoadScene("TransitionScene");
            Debug.Log("[ArticleViewerScene]: Participant has finished reading the article.");
        });

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

        // Disable Continue if fewer than 5 articles have been read
        if (tracker != null && tracker.selectedArticles.articles.Count < 5)
        {
            continueButton.interactable = false;
            Debug.Log("[ArticleViewerScene]: Continue disabled, less than 5 articles read.");
        }
        else
        {
            continueButton.interactable = true;
            Debug.Log("[ArticleViewerScene]: Continue enabled.");
        }
    }
}