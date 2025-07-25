using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SurveyScreen : MonoBehaviour
{
    public TMP_InputField feedbackInput;
    public TMP_InputField ageInput;
    public Button submitButton;
    private float startTime;

    private void Start()
    {
        startTime = Time.time;
        submitButton.onClick.AddListener(SubmitSurvey);
    }

    private void SubmitSurvey()
    {
        string feedback = feedbackInput.text;
        string ageText = ageInput.text;
        float duration = Time.time - startTime;

        if (string.IsNullOrWhiteSpace(ageText))
        {
            Debug.LogWarning("Age is required!");
            return;
        }

        if (!int.TryParse(ageText, out int age))
        {
            Debug.LogWarning("Invalid age entered.");
            return;
        }

        Debug.Log($"[SurveyScene]: Participant took {duration:F2} seconds to submit feedback.");

        // Save to participant data
        QuestionScreen.participantData.surveySceneDuration = duration.ToString("F2") + " seconds";
        QuestionScreen.participantData.age = age;
        QuestionScreen.participantData.feedback = feedback;

        // Move to Thank You scene
        SceneManager.LoadScene("ThankYouScene");
    }
}