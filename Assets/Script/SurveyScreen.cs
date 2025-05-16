using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class SurveyScreen : MonoBehaviour
{
    public TMP_InputField feedbackInput;
    public TMP_InputField studentIdInput;
    public TMP_InputField ageInput;
    public Button submitButton;

    private void Start()
    {
        submitButton.onClick.AddListener(SubmitSurvey);
    }

    private void SubmitSurvey()
    {
        string feedback = feedbackInput.text;
        string studentId = studentIdInput.text;
        string ageText = ageInput.text;

        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(ageText))
        {
            Debug.LogWarning("Student ID and Age are required!");
            return;
        }

        int age;
        if (!int.TryParse(ageText, out age))
        {
            Debug.LogWarning("Invalid age entered.");
            return;
        }

        // Set participant info
        QuestionScreen.participantData.studentID = studentId;
        QuestionScreen.participantData.age = age;
        QuestionScreen.participantData.feedback = feedback;

        // Convert to JSON
        string json = JsonUtility.ToJson(QuestionScreen.participantData, true);

        // Get user home folder dynamically
        string userHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string folderPath = Path.Combine(userHome, "confirmationBiasinJSON");

        // Create folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("Created folder: " + folderPath);
        }

        // Create full file path
        string filename = $"participant_{studentId}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string fullPath = Path.Combine(folderPath, filename);

        // Write JSON to file
        File.WriteAllText(fullPath, json);

        Debug.Log("Participant data saved to: " + fullPath);

        // Load Thank You scene
        SceneManager.LoadScene("ThankYouScene");
    }
}
