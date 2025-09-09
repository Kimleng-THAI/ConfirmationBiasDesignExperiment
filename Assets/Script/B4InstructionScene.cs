using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class B4InstructionScene : MonoBehaviour
{
    private PlayerInputActions inputActions;

    [Header("Next Scene")]
    [Tooltip("Name of the scene to load after pressing SPACE.")]
    public string nextSceneName = "DemoScene";

    void Awake()
    {
        inputActions = new PlayerInputActions();
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

    private void OnContinuePressed(InputAction.CallbackContext context)
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[B4InstructionScene] nextSceneName is not set.");
        }
    }
}
