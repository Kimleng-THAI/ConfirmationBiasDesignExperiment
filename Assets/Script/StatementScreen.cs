using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StatementScreen : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private float startTime;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += OnContinuePressed;

        // Record the time when the scene becomes active
        startTime = Time.time;
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
        Debug.Log($"[StatementScene]: Time taken to press SPACE: {reactionTimeStr}");

        // Save to participant data
        QuestionScreen.participantData.statementSceneReactionTime = reactionTimeStr;

        // Replace with the next scene
        SceneManager.LoadScene("QuestionScene");
    }
}
