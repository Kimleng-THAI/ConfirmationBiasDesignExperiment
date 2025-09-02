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
                "You have now finished the first part of the experiment!\n" +
                "You will now be presented with a topic dropdown to select an article to read.\n" +
                "The study will be completed once you read at least 2 articles per topic.\n" +
                "A topic will be marked as `completed` once you have met this requirement for that topic.\n" +
                "The study will be deemed complete when all topics are marked as complete.\n\n" +

                "You can scroll through all articles in the selected topic.\n" +
                "After reading each article, you will be asked to provide your level of agreement.\n" +
                "After providing your level of agreement, you will press:\n" +

                "- Y = Yes\n" +
                "- N = No\n\n" +

                "After the attention check:\n" +
                "- Press the Left Arrow Key (`<-`) to go back to the dropdown menu.\n" +
                "- Press the Right Arrow Key (`->`) when you have completed the reading requirements.\n\n" +

                "You may read as many articles as you wish; however, an option will be provided to end the experiment once ten articles in total have been read (for example, two articles for five topics).\n" +
                "We encourage you to select topics from the dropdown that interest you the most.\n\n" +
                "Please press SPACE to begin the second part of the experiment!\n";
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
        LogEvent("ArticleInstructions_CONTINUE_PRESSED");
        PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
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