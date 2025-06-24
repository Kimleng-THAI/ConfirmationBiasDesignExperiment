using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

        if (!int.TryParse(ageText, out int age))
        {
            Debug.LogWarning("Invalid age entered.");
            return;
        }

        // Set participant info (but do NOT save to file yet)
        QuestionScreen.participantData.studentID = studentId;
        QuestionScreen.participantData.age = age;
        QuestionScreen.participantData.feedback = feedback;

        // Move to Thank You scene
        SceneManager.LoadScene("ThankYouScene");
    }
}