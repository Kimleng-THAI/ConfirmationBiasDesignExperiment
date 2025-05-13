using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThankYouScreen : MonoBehaviour
{
    public Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(ExitExperiment);
    }

    private void ExitExperiment()
    {
        // Load a final scene or quit the application
        Debug.Log("Participant has finished the experiment.");

        // Optionally, you can exit the application after the feedback:
        Application.Quit();

        // Or load another scene, for example, the main menu
        // SceneManager.LoadScene("BeliefInputScene");
    }
}
