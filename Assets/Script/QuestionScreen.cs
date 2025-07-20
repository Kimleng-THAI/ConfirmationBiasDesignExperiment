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

    private Coroutine eegCoroutine;
    private Coroutine heartRateCoroutine;
    private float experimentStartTimeRealtime;

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
        inputActions.UI.Select5.performed += OnSelect5;

        startTime = Time.time;
        experimentStartTimeRealtime = Time.realtimeSinceStartup;
        eegCoroutine = StartCoroutine(SimulateEEGData());
        heartRateCoroutine = StartCoroutine(SimulateHeartRateData());
        LoadQuestion(currentQuestionIndex);
    }

    private void OnDisable()
    {
        inputActions.UI.Select1.performed -= OnSelect1;
        inputActions.UI.Select2.performed -= OnSelect2;
        inputActions.UI.Select3.performed -= OnSelect3;
        inputActions.UI.Select4.performed -= OnSelect4;
        inputActions.UI.Select5.performed -= OnSelect5;

        inputActions.UI.Disable();

        if (eegCoroutine != null) StopCoroutine(eegCoroutine);
        if (heartRateCoroutine != null) StopCoroutine(heartRateCoroutine);
    }

    private void OnSelect1(InputAction.CallbackContext ctx) => RecordResponse("1");
    private void OnSelect2(InputAction.CallbackContext ctx) => RecordResponse("2");
    private void OnSelect3(InputAction.CallbackContext ctx) => RecordResponse("3");
    private void OnSelect4(InputAction.CallbackContext ctx) => RecordResponse("4");
    private void OnSelect5(InputAction.CallbackContext ctx) => RecordResponse("5");

    private void LoadQuestion(int index)
    {
        if (index >= questions.Count)
        {
            Debug.Log("[QuestionScene]: All questions completed!");
            SceneManager.LoadScene("TopicSelectorScene");
            return;
        }

        var q = questions[index];
        conflictStatementText.text = $"<b>{q.topic}</b>\n\n{q.statement}";

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = (i < q.options.Count) ? q.options[i] : "";
        }
    }

    private void RecordResponse(string option)
    {
        float rawReactionTime = Time.time - startTime;
        string reactionTime = rawReactionTime.ToString("F3");

        Debug.Log($"[Question {currentQuestionIndex}] Option {option} selected after {reactionTime} seconds.");

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

    private IEnumerator<WaitForSeconds> SimulateEEGData()
    {
        while (true)
        {
            float timestamp = Time.realtimeSinceStartup - experimentStartTimeRealtime;
            float microvolts = Random.Range(10f, 100f); // Simulated EEG signal strength

            QuestionScreen.participantData.eegReadings.Add(new EEGReading
            {
                timestamp = timestamp,
                microvolts = microvolts
            });

            yield return new WaitForSeconds(0.1f); // 10Hz EEG
        }
    }

    private IEnumerator<WaitForSeconds> SimulateHeartRateData()
    {
        while (true)
        {
            float timestamp = Time.realtimeSinceStartup - experimentStartTimeRealtime;
            int bpm = Random.Range(60, 100); // Simulated heart rate

            QuestionScreen.participantData.heartRateReadings.Add(new HeartRateReading
            {
                timestamp = timestamp,
                bpm = bpm
            });

            yield return new WaitForSeconds(1f); // 1Hz heart rate
        }
    }
}
