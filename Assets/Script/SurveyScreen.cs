using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SurveyScreen : MonoBehaviour
{
    public TMP_InputField feedbackInput;
    public Button submitButton;

    private void Start()
    {
        submitButton.onClick.AddListener(SubmitFeedback);
    }

    private void SubmitFeedback()
    {
        string feedback = feedbackInput.text;
        Debug.Log("Participant Feedback: " + feedback);

        // Optional: Save feedback locally
        string path = Application.persistentDataPath + "/feedback.txt";
        System.IO.File.AppendAllText(path, feedback + "\n");

        // Load a "Thank You" screen or return to main
        SceneManager.LoadScene("ThankYouScene");
    }
}
