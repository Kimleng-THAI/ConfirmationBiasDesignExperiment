using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class QuestionScreen : MonoBehaviour
{
    private PlayerInputActions inputActions;
    private float startTime;

    public TextMeshProUGUI conflictStatementText;
    public TextMeshProUGUI option1Text;
    public TextMeshProUGUI option2Text;
    public TextMeshProUGUI option3Text;
    public TextMeshProUGUI option4Text;
    public TextMeshProUGUI promptText;

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

        // Example placeholders – replace with actual logic or dynamic content
        conflictStatementText.text = "Some argue that Michael Jordan’s era had tougher competition, making his dominance more impressive.";
        option1Text.text = "1. Jordan never had to face the modern athleticism LeBron goes against every night.";
        option2Text.text = "2. LeBron’s ability to excel in any team shows his superior adaptability.";
        option3Text.text = "3. Jordan faced more physical defenses and still came out on top.";
        option4Text.text = "4. MJ’s six championships without a Game 7 show unmatched dominance.";
        promptText.text = "Press 1, 2, 3, or 4 to select your answer.";
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

        // TODO: Store the result or move to the next scene
    }
}
