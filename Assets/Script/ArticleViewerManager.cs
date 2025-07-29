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

    // Timestamp for current scene
    private float sceneStartTime;

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        backButton.onClick.AddListener(() =>
        {
            LogEvent("BackButtonClicked");
            PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
            SceneManager.LoadScene("TransitionScene");
        });

        continueButton.onClick.AddListener(() =>
        {
            LogEvent("ContinueButtonClicked");
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

    private void LogEvent(string label)
    {
        if (QuestionScreen.participantData != null)
        {
            float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
            float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"{label} (local: {localTimestamp:F2}s)"
            });

            Debug.Log($"[ArticleViewerScene] Event Logged: {label} | Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
        }
        else
        {
            Debug.LogWarning("[ArticleViewerScene] Could not log event: participantData is null.");
        }
    }
}