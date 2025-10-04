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
            "You have now completed phase one of the experiment!\n\n" +
            "In phase two, you will choose articles from different topics to read.\n\n" +
            "After reading each article, you will rate your level of agreement and then complete a short quiz.\n\n" +
            "You will be given an option once you have read at least 2 articles from each topic and a total of 10 articles overall.\n\n" +
            "After this instruction, a short demonstration screenshot will show you how the phase two works.\n\n" +
            "There will be a series of demonstration screenshots. Press the Spacebar to move from one screenshot to the next until the demonstration is complete.\n\n" +
            "Press Spacebar to see the demonstration screenshot.";
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
        SceneManager.LoadScene("ArticleInstructionsScene");
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