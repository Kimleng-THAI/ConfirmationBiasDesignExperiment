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
            ParticipantData.ParticipantBelief = input;
            SceneManager.LoadScene("InstructionScreen");
        }
        else
        {
            Debug.Log("Please enter a belief before continuing.");
        }
    }
}
