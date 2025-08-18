using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // needed for TextMeshPro

public class SceneTransitionManager : MonoBehaviour
{
    public float transitionDelay = 1.0f;
    public TextMeshProUGUI transitionText;

    private void Start()
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterTransition", "");

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("No next scene specified! Did you forget to set PlayerPrefs?");
            return;
        }

        Debug.Log($"[TransitionScene] Waiting {transitionDelay} seconds to load '{nextScene}'...");

        Invoke("LoadNextScene", transitionDelay);
    }

    private void Update()
    {
        // Keep updating the "X" while in transition
        if (transitionText != null)
        {
            transitionText.text = "X";
        }
    }

    void LoadNextScene()
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterTransition");
        SceneManager.LoadScene(nextScene);
    }
}