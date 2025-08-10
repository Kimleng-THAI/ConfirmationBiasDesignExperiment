using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ArticleInstructionsManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI instructionText;

    private float sceneStartTime;
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        if (instructionText != null && string.IsNullOrEmpty(instructionText.text))
        {
            instructionText.text =
                "You must read 2 articles per topic.\n" +
                "You will also have to provide your level of agreement after reading each article.\n\n" +
                "Press the Left Arrow Key (`<-`) to go back to the previous scene.\n" +
                "Press the Right Arrow Key (`->`) to continue to the next scene.\n\n";
        }
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.GoBack.performed += OnBackKeyPressed;
        inputActions.UI.GoForward.performed += OnForwardKeyPressed;
    }

    void OnDisable()
    {
        inputActions.UI.GoBack.performed -= OnBackKeyPressed;
        inputActions.UI.GoForward.performed -= OnForwardKeyPressed;
        inputActions.UI.Disable();
    }

    // Key handlers
    private void OnBackKeyPressed(InputAction.CallbackContext ctx) => NavigateToPrevious();
    private void OnForwardKeyPressed(InputAction.CallbackContext ctx) => NavigateToNext();

    // Optional UI button handlers
    public void OnBackButtonClicked() => NavigateToPrevious();
    public void OnContinueButtonClicked() => NavigateToNext();

    // Navigation (uses your TransitionScene pattern)
    private void NavigateToPrevious()
    {
        LogEvent("ArticleInstructions_BACK_PRESSED");
        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private void NavigateToNext()
    {
        LogEvent("ArticleInstructions_CONTINUE_PRESSED");
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    private void LogEvent(string label)
    {
        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = 0f;
        try
        {
            globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;
        }
        catch { /* keep 0 if singleton not present - optional safety */ }

        if (QuestionScreen.participantData != null)
        {
            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"{label} (local: {localTimestamp:F2}s)"
            });
        }

        Debug.Log($"[ArticleInstructions] {label} — Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
    }
}