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
                "You will now be presented with a dropdown menu to select a topic from which a selection of articles will be presented on the next screen.\n" +
                "You can scroll through all articles in the selected topic.\n" +
                "Once the 'Read article' button is clicked for a headline of your choice, after some time passes, you will be asked to provide your level of alignment with the presented information.\n" +
                "After providing your alignment rating, as before, you will be asked to answer a quick attention check with:\n" +

                "You can scroll through all articles in the selected topic.\n" +
                "After reading each article, you will be asked to provide your level of agreement.\n" +
                "After providing your level of agreement, you will press:\n" +

                "- Y = Yes\n" +
                "- N = No\n\n" +

                "Once you have answered the attention check, the experiment will go back to the initial topic selection dropdown screen for you to make your choice as to what article to read next\n" +
                "If you have any questions, you may ask them now, or otherwise when the topic dropdown menu screen is presented.\n" +
                "You may read as many articles as you wish; however, an option will be provided to end the experiment once ten articles in total have been read (for example, two articles for five topics).\n" +

                "We encourage you to select topics from the dropdown that interest you the most.\n" +
                "Note: A topic will be marked as `(completed)` once you read two articles for that topic.\n\n" +
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