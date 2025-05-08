using UnityEngine;
using UnityEngine.InputSystem;  // Use new Input System namespace
using UnityEngine.SceneManagement;

public class InstructionScreen : MonoBehaviour
{
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Continue.performed += ctx => LoadNextScene();
    }

    private void OnDisable()
    {
        inputActions.UI.Disable();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene("StatementScene");
    }
}
