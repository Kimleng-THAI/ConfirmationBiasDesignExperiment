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
        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    private void OnDisable()
    {
        // Important to unsubscribe
        inputActions.UI.Continue.performed -= OnContinuePressed;
        inputActions.UI.Disable();
    }

    private void OnContinuePressed(InputAction.CallbackContext context)
    {
        float reactionTime = Time.time - startTime;
        Debug.Log($"[InstructionScreen]: Time taken to press SPACE: {reactionTime:F3} seconds.");

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene("StatementScene");
    }
}
