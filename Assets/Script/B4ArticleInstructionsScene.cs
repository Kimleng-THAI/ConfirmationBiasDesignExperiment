using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class B4ArticleInstructionsScene : MonoBehaviour
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
                instructionText.text =
            "You have now completed the first part of the experiment!\n\n" +
            "In the next phase, you will choose articles from different topics to read.\n" +
            "After reading each article, you will rate your level of agreement and then complete a short attention check.\n\n" +
            "You may read as many articles as you wish; however, an option will be provided to end the experiment once ten articles in total have been read (for example, two articles for five topics).\n\n" +
            "After this instruction, a short demonstration video will show you how the experiment works.\n\n" +
            "Press SPACE to see the demonstration video.";
        }
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    void OnDisable()
    {
        inputActions.UI.Continue.performed -= OnContinuePressed;
        inputActions.UI.Disable();
    }

    // Key handler
    private void OnContinuePressed(InputAction.CallbackContext ctx) => NavigateToNext();

    // Optional UI button handler
    public void OnContinuePressed() => NavigateToNext();

    private void NavigateToNext()
    {
        LogEvent("B4ArticleInstructionsScene_CONTINUE_PRESSED");
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleInstructionsScene");
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
        catch
        {
            // keep 0 if singleton not present - optional safety
        }

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