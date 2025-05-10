using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class QuestionScreen : MonoBehaviour
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

        inputActions.UI.Select1.performed += ctx => RecordResponse("1");
        inputActions.UI.Select2.performed += ctx => RecordResponse("2");
        inputActions.UI.Select3.performed += ctx => RecordResponse("3");
        inputActions.UI.Select4.performed += ctx => RecordResponse("4");

        startTime = Time.time;
    }

    private void OnDisable()
    {
        inputActions.UI.Select1.performed -= ctx => RecordResponse("1");
        inputActions.UI.Select2.performed -= ctx => RecordResponse("2");
        inputActions.UI.Select3.performed -= ctx => RecordResponse("3");
        inputActions.UI.Select4.performed -= ctx => RecordResponse("4");

        inputActions.UI.Disable();
    }

    private void RecordResponse(string option)
    {
        float reactionTime = Time.time - startTime;
        Debug.Log($"[QuestionScene] Selected Option {option} after {reactionTime:F3} seconds.");

        // Load next question scene
        SceneManager.LoadScene("QuestionScene2");
    }
}
