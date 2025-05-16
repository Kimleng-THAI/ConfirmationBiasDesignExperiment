using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class QuestionScreen : MonoBehaviour
{
    public TextMeshProUGUI conflictStatementText;
    public TextMeshProUGUI[] optionTexts;

    private PlayerInputActions inputActions;
    private float startTime;
    private static int currentQuestionIndex = 0;
    private List<Question> questions;

    public static ParticipantData1 participantData = new ParticipantData1(); // moved inside class

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        QuestionList qList = QuestionDataLoader.LoadQuestionsFromJSON();

        if (qList == null || qList.questions == null || qList.questions.Count == 0)
        {
            Debug.LogError("No questions loaded. Check your JSON file.");
            return;
        }

        questions = qList.questions;
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();

        inputActions.UI.Select1.performed += OnSelect1;
        inputActions.UI.Select2.performed += OnSelect2;
        inputActions.UI.Select3.performed += OnSelect3;
        inputActions.UI.Select4.performed += OnSelect4;

        startTime = Time.time;
        LoadQuestion(currentQuestionIndex);
    }

    private void OnDisable()
    {
        inputActions.UI.Select1.performed -= OnSelect1;
        inputActions.UI.Select2.performed -= OnSelect2;
        inputActions.UI.Select3.performed -= OnSelect3;
        inputActions.UI.Select4.performed -= OnSelect4;

        inputActions.UI.Disable();
    }

    private void OnSelect1(InputAction.CallbackContext ctx) => RecordResponse("1");
    private void OnSelect2(InputAction.CallbackContext ctx) => RecordResponse("2");
    private void OnSelect3(InputAction.CallbackContext ctx) => RecordResponse("3");
    private void OnSelect4(InputAction.CallbackContext ctx) => RecordResponse("4");

    private void LoadQuestion(int index)
    {
        if (index >= questions.Count)
        {
            Debug.Log("All questions completed!");
            SceneManager.LoadScene("SurveyScene");
            return;
        }

        var q = questions[index];
        conflictStatementText.text = q.conflictStatement;

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = (i < q.options.Count) ? q.options[i] : "";
        }
    }

    private void RecordResponse(string option)
    {
        float reactionTime = Time.time - startTime;
        var currentQuestion = questions[currentQuestionIndex];

        Debug.Log($"[Question {currentQuestionIndex}] Option {option} selected after {reactionTime:F3} seconds.");

        // Save response data
        var response = new ResponseRecord
        {
            questionIndex = currentQuestionIndex,
            selectedOption = option,
            reactionTime = reactionTime
        };
        participantData.responses.Add(response);

        currentQuestionIndex++;
        startTime = Time.time; // reset timer for next question
        LoadQuestion(currentQuestionIndex);
    }
}
