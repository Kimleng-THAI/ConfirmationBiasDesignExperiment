using UnityEngine;
using UnityEngine.InputSystem;

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

        inputActions.UI.Select1.performed += ctx => RecordResponse("A");
        inputActions.UI.Select2.performed += ctx => RecordResponse("B");
        inputActions.UI.Select3.performed += ctx => RecordResponse("C");
        inputActions.UI.Select4.performed += ctx => RecordResponse("D");

        startTime = Time.time;
    }

    private void OnDisable()
    {
        inputActions.UI.Select1.performed -= ctx => RecordResponse("A");
        inputActions.UI.Select2.performed -= ctx => RecordResponse("B");
        inputActions.UI.Select3.performed -= ctx => RecordResponse("C");
        inputActions.UI.Select4.performed -= ctx => RecordResponse("D");

        inputActions.UI.Disable();
    }

    private void RecordResponse(string option)
    {
        float reactionTime = Time.time - startTime;
        Debug.Log($"[QuestionScene] Selected Option {option} after {reactionTime:F3} seconds.");

        // We can now store the response if needed, or proceed
        // SceneManager.LoadScene("NextSceneName");
    }
}
