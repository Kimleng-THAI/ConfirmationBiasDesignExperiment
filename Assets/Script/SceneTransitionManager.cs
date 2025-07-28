using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public float transitionDelay = 1.0f;

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

    void LoadNextScene()
    {
        string nextScene = PlayerPrefs.GetString("NextSceneAfterTransition");
        SceneManager.LoadScene(nextScene);
    }
}