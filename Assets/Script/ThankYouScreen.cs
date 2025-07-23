using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;

public class ThankYouScreen : MonoBehaviour
{
    public Button exitButton;
    private float startThankYouScene;

    private void Start()
    {
        startThankYouScene = Time.time;
        exitButton.onClick.AddListener(ExitExperiment);
    }

    private void ExitExperiment()
    {
        float recordTimeTaken = Time.time - startThankYouScene;
        Debug.Log($"[ThankYouScene]: Participant took {recordTimeTaken:F2} seconds to press Done button.");

        // 1. Save selected topic
        string selectedTopic = PlayerPrefs.GetString("SelectedTopic", "Not Selected");
        QuestionScreen.participantData.selectedFinalTopic = selectedTopic;

        // 2. Load full list of selected articles
        SelectedArticleList fullArticleList = null;

        ArticleSelectionTracker tracker = FindFirstObjectByType<ArticleSelectionTracker>();
        if (tracker != null && tracker.selectedArticles != null && tracker.selectedArticles.articles.Count > 0)
        {
            fullArticleList = tracker.selectedArticles;
            Debug.Log("[ThankYouScene]: Loaded full article history from tracker.");
        }
        else
        {
            string json = PlayerPrefs.GetString("SelectedArticles", "");
            if (!string.IsNullOrEmpty(json))
            {
                SelectedArticleList fallbackList = JsonUtility.FromJson<SelectedArticleList>(json);
                if (fallbackList != null && fallbackList.articles.Count > 0)
                {
                    fullArticleList = fallbackList;
                    Debug.Log("[ThankYouScene]: Loaded full article history from PlayerPrefs.");
                }
            }
        }

        if (fullArticleList != null)
        {
            // Assign full list of selected articles to participantData
            QuestionScreen.participantData.selectedArticles = fullArticleList.articles;

            // Optionally, also update participantData's last article headline/content/topic fields for convenience
            //SelectedArticle lastArticle = fullArticleList.articles[^1];
            //QuestionScreen.participantData.selectedArticleHeadline = lastArticle.headline;
            //QuestionScreen.participantData.selectedArticleContent = lastArticle.content;
            //if (!string.IsNullOrEmpty(lastArticle.topic))
            //{
            //    QuestionScreen.participantData.selectedTopic = lastArticle.topic;
            //}
        }
        else
        {
            // No articles found - clear or set default messages
            QuestionScreen.participantData.selectedArticles = new System.Collections.Generic.List<SelectedArticle>();
            //QuestionScreen.participantData.selectedArticleHeadline = "No article selected";
            //QuestionScreen.participantData.selectedArticleContent = "No content available";
        }

        // 3. Record thank you screen duration
        QuestionScreen.participantData.thankYouSceneDuration = $"{recordTimeTaken:F2} seconds";

        // 4. Get current time in Sydney
        DateTime utcNow = DateTime.UtcNow;
        DateTime sydneyTime;

        try
        {
            TimeZoneInfo sydneyTimeZone = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor
                ? TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time")
                : TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");

            sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, sydneyTimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            Debug.LogWarning("Sydney timezone not found. Falling back to UTC.");
            sydneyTime = utcNow;
        }

        QuestionScreen.participantData.experimentEndTime = sydneyTime.ToString("yyyy-MM-dd HH:mm:ss") + " AEST";

        // 5. Calculate total experiment duration
        if (DateTime.TryParse(QuestionScreen.participantData.experimentStartTime.Replace(" AEST", ""), out DateTime startTime))
        {
            TimeSpan totalDuration = sydneyTime - startTime;
            QuestionScreen.participantData.duration = $"{(int)totalDuration.TotalMinutes} minutes and {totalDuration.Seconds} seconds";
            Debug.Log($"Experiment duration: {QuestionScreen.participantData.duration}");
        }
        else
        {
            Debug.LogWarning("Could not parse experimentStartTime.");
        }

        // 6. Save participant data to JSON
        string jsonOutput = JsonUtility.ToJson(QuestionScreen.participantData, true);
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "confirmationBiasinJSON");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Created folder: " + folderPath);
        }

        string studentId = QuestionScreen.participantData.studentID ?? "unknown";
        string filename = $"participant_{studentId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(folderPath, filename);

        File.WriteAllText(fullPath, jsonOutput);
        Debug.Log("Participant data saved to: " + fullPath);

        // 7. Quit or transition
        // Application.Quit(); // Uncomment to quit after saving
    }
}