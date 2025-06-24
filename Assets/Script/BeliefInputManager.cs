using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BeliefInputManager : MonoBehaviour
{
    public TMP_InputField beliefInputField;

    public void OnContinueButtonClicked()
    {
        string input = beliefInputField.text;

        if (!string.IsNullOrEmpty(input))
        {
            // Store belief and start time in main data object
            var data = QuestionScreen.participantData;
            data.belief = input;
            // ISO format
            System.DateTime sydneyTime = System.DateTime.UtcNow.AddHours(10);
            data.experimentStartTime = sydneyTime.ToString("yyyy-MM-dd HH:mm:ss") + " AEST";
            SceneManager.LoadScene("InstructionScreen");
        }
        else
        {
            Debug.Log("Please enter a belief before continuing.");
        }
    }
}
