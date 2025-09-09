using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeveloperInputManager : MonoBehaviour
{
    public TMP_InputField subjectNumberInput;
    public string nextSceneName = "B4InstructionScene";

    public void OnContinueButtonClicked()
    {
        string subjectNumber = subjectNumberInput.text;

        if (string.IsNullOrEmpty(subjectNumber))
        {
            Debug.LogWarning("Subject number is empty!");
            return;
        }

        PlayerPrefs.SetString("SubjectNumber", subjectNumber);
        PlayerPrefs.SetString("NextSceneAfterTransition", nextSceneName);
        PlayerPrefs.Save();

        Debug.Log($"Subject Number stored: {subjectNumber}");
        Debug.Log("Loading TransitionScene...");

        SceneManager.LoadScene("TransitionScene");
    }
}