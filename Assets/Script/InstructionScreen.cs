using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InstructionScreen : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private float startTime;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Record the time when the instruction screen is shown
        startTime = Time.time;

        // Record experiment start time (in Sydney time)
        System.DateTime sydneyTime = System.DateTime.UtcNow.AddHours(10);
        QuestionScreen.participantData.experimentStartTime = sydneyTime.ToString("yyyy-MM-dd HH:mm:ss") + " AEST";
        Debug.Log("[InstructionScreen]: Experiment start time set to " + QuestionScreen.participantData.experimentStartTime);

        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    private void OnDisable()
    {
        inputActions.UI.Continue.performed -= OnContinuePressed;
        inputActions.UI.Disable();
    }

    private void OnContinuePressed(InputAction.CallbackContext context)
    {
        float reactionTime = Time.time - startTime;
        string reactionTimeStr = reactionTime.ToString("F3") + " seconds";
        Debug.Log($"[InstructionScreen]: Time taken to press SPACE: {reactionTimeStr}");

        // Save to participant data
        QuestionScreen.participantData.instructionScreenReactionTime = reactionTimeStr;

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene("QuestionScene");
    }
}