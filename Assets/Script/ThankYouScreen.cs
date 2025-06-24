using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;

public class ThankYouScreen : MonoBehaviour
{
    public Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(ExitExperiment);
    }

    private void ExitExperiment()
    {
        Debug.Log("Participant has finished the experiment.");

        // Safely convert to Sydney time (works across OS)
        DateTime utcNow = DateTime.UtcNow;
        DateTime sydneyTime;

        try
        {
            // Use "Australia/Sydney" for macOS/Linux, "AUS Eastern Standard Time" for Windows
            TimeZoneInfo sydTimeZone;
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                sydTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            }
            else
            {
                sydTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney");
            }

            sydneyTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, sydTimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            Debug.LogWarning("Sydney timezone not found. Using UTC as fallback.");
            sydneyTime = utcNow;
        }

        // Save end time
        QuestionScreen.participantData.experimentEndTime = sydneyTime.ToString("yyyy-MM-dd HH:mm:ss") + " AEST";

        // Calculate duration
        DateTime startTime;
        if (DateTime.TryParse(QuestionScreen.participantData.experimentStartTime.Replace(" AEST", ""), out startTime))
        {
            TimeSpan duration = sydneyTime - startTime;
            QuestionScreen.participantData.duration = $"{(int)duration.TotalMinutes} minutes and {duration.Seconds} seconds";
            Debug.Log($"Experiment completed in {duration.Minutes} minutes and {duration.Seconds} seconds.");
        }
        else
        {
            Debug.LogWarning("Could not parse experimentStartTime.");
        }

        // Convert to JSON
        string json = JsonUtility.ToJson(QuestionScreen.participantData, true);

        // Save file to ~/confirmationBiasinJSON/
        string userHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string folderPath = Path.Combine(userHome, "confirmationBiasinJSON");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Created folder: " + folderPath);
        }

        string studentId = QuestionScreen.participantData.studentID ?? "unknown";
        string filename = $"participant_{studentId}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(folderPath, filename);

        File.WriteAllText(fullPath, json);
        Debug.Log("Participant data saved to: " + fullPath);

        // Optionally quit or load another scene
        // Application.Quit();
    }
}