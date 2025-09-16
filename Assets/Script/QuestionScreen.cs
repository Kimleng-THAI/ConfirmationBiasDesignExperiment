using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using LSL;

public class QuestionScreen : MonoBehaviour
{
    public GameObject blankOverlay;

    public TextMeshProUGUI conflictStatementText;
    public TextMeshProUGUI[] optionTexts;

    private PlayerInputActions inputActions;
    private static int currentQuestionIndex = 0;
    private List<Question> questions;

    public static float experimentStartTimeRealtime;

    public static ParticipantData1 participantData = new ParticipantData1();

    private float questionStartTimeRealtime;
    private Coroutine questionTimerCoroutine;
    private bool hasRespondedQuestion = false;

    private string tempSelectedOption = "";

    private bool isResting = false;
    private float restStartTimeRealtime;

    public GameObject restBreakOverlay;
    public TextMeshProUGUI restBreakMessageText;

    public TextMeshProUGUI transitionXText;
    public TextMeshProUGUI attentionCheckText;

    private bool isAttentionCheckActive = false;
    private bool hasRespondedAttentionCheck = false;
    private Coroutine attentionCheckTimerCoroutine;
    private Question currentAttentionCheckQuestion;
    private float attentionCheckStartTimeRealtime;

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

        // Randomise the question order
        for (int i = 0; i < questions.Count; i++)
        {
            Question temp = questions[i];
            int randomIndex = Random.Range(i, questions.Count);
            questions[i] = questions[randomIndex];
            questions[randomIndex] = temp;
        }
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();

        inputActions.UI.Select1.performed += OnSelect1;
        inputActions.UI.Select2.performed += OnSelect2;
        inputActions.UI.Select3.performed += OnSelect3;
        inputActions.UI.Select4.performed += OnSelect4;
        inputActions.UI.Select5.performed += OnSelect5;

        inputActions.UI.GoBack.performed += OnAttentionCheckLeft;
        inputActions.UI.GoForward.performed += OnAttentionCheckRight;

        experimentStartTimeRealtime = Time.realtimeSinceStartup;
        

        if (blankOverlay != null) blankOverlay.SetActive(false);
        if (transitionXText != null) transitionXText.gameObject.SetActive(false);
        if (attentionCheckText != null) attentionCheckText.gameObject.SetActive(false);

        LoadQuestion(currentQuestionIndex);
    }

    private void OnDisable()
    {
        inputActions.UI.Select1.performed -= OnSelect1;
        inputActions.UI.Select2.performed -= OnSelect2;
        inputActions.UI.Select3.performed -= OnSelect3;
        inputActions.UI.Select4.performed -= OnSelect4;
        inputActions.UI.Select5.performed -= OnSelect5;

        inputActions.UI.GoBack.performed -= OnAttentionCheckLeft;
        inputActions.UI.GoForward.performed -= OnAttentionCheckRight;

        inputActions.UI.Disable();

        
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
            StartCoroutine(TransitionToB4ArticleInstructionsScene());
            return;
        }

        questionStartTimeRealtime = Time.realtimeSinceStartup;
        hasRespondedQuestion = false;

        if (questionTimerCoroutine != null) StopCoroutine(questionTimerCoroutine);
        questionTimerCoroutine = StartCoroutine(QuestionTimer());

        var q = questions[index];
        conflictStatementText.gameObject.SetActive(true);
        conflictStatementText.text = $"\n\n{q.statement}";

        // ✅ Force font size for statement
        conflictStatementText.enableAutoSizing = false;
        conflictStatementText.fontSize = 40; // adjust as you like

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i < q.options.Count)
            {
                optionTexts[i].gameObject.SetActive(true);
                optionTexts[i].text = q.options[i];

                // ✅ Force font size for options
                optionTexts[i].enableAutoSizing = false;
                optionTexts[i].fontSize = 36; // adjust as you like
            }
            else
            {
                optionTexts[i].gameObject.SetActive(false);
            }
        }

        // ✅ Send LSL marker when statement is presented
        LSLManager.Instance.SendMarker($"STATEMENT_PRESENTED_Q{index}_{q.topicCode}_{q.statementCode}");
    }

    private void RecordResponse(string option)
    {
        if (isResting || hasRespondedQuestion || isAttentionCheckActive) return;

        hasRespondedQuestion = true;

        if (questionTimerCoroutine != null) StopCoroutine(questionTimerCoroutine);

        var q = questions[currentQuestionIndex];
        float rawReactionTime = Time.realtimeSinceStartup - questionStartTimeRealtime;
        tempSelectedOption = option;

        participantData.responses.Add(new ResponseRecord
        {
            questionIndex = currentQuestionIndex,
            topicCode = q.topicCode,
            statementCode = q.statementCode,
            selectedOption = tempSelectedOption,
            agreementReactionTime = rawReactionTime.ToString("F3")
        });

        Debug.Log($"[Question {currentQuestionIndex}] {q.topicCode}-{q.statementCode} | Agreement Selected Option: {tempSelectedOption} after {rawReactionTime:F3} seconds.");

        float localTimestamp = Time.realtimeSinceStartup - questionStartTimeRealtime;
        float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

        participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = $"[QuestionScene]: Question: {currentQuestionIndex} | TopicCode: {q.topicCode} | StatementCode: {q.statementCode} | Agreement_Level: {option}"
        });

        Debug.Log($"[QuestionScene]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: Question_{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_AGREEMENT_{option}");

        // ✅ Send LSL marker and Likert response
        LSLManager.Instance.SendMarker($"STATEMENT_RESPONSE_Q{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_AGREEMENT_{option}");
        LSLManager.Instance.SendLikertResponse(1, currentQuestionIndex, int.Parse(option), rawReactionTime);

        if (q.check != null)
        {
            currentAttentionCheckQuestion = q;
            StartAttentionCheck(q);
        }
        else
        {
            tempSelectedOption = "";
            currentQuestionIndex++;
            ProceedToNext();
        }
    }

    private void StartAttentionCheck(Question q)
    {
        isAttentionCheckActive = true;
        hasRespondedAttentionCheck = false;

        conflictStatementText.gameObject.SetActive(false);
        foreach (var optionText in optionTexts)
            optionText.gameObject.SetActive(false);

        if (attentionCheckText != null)
        {
            attentionCheckText.text =
                $"Did the statement contain the word '{q.check.word}'?\n\n" +
                "<size=80%><color=#FFFFFF>Press LEFT ARROW KEY = No, RIGHT ARROW KEY = Yes</color></size>";
            attentionCheckText.gameObject.SetActive(true);
        }

        attentionCheckStartTimeRealtime = Time.realtimeSinceStartup;

        // ✅ Send LSL marker for attention check presentation
        LSLManager.Instance.SendMarker($"ATTENTION_CHECK_PRESENTED_Q{currentQuestionIndex}_{q.topicCode}_{q.statementCode}");

        if (attentionCheckTimerCoroutine != null) StopCoroutine(attentionCheckTimerCoroutine);
        attentionCheckTimerCoroutine = StartCoroutine(AttentionCheckTimer(q, 5f));
    }

    private IEnumerator<WaitForSeconds> AttentionCheckTimer(Question q, float timeLimit)
    {
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime < timeLimit)
        {
            if (hasRespondedAttentionCheck)
                yield break;
            yield return null;
        }

        if (!hasRespondedAttentionCheck)
        {
            LogAttentionResponse("ATTENTION_FAIL");
            EndAttentionCheck();
        }
    }

    private void OnAttentionCheckLeft(InputAction.CallbackContext ctx)
    {
        if (!isAttentionCheckActive || hasRespondedAttentionCheck) return;

        hasRespondedAttentionCheck = true;

        string response = currentAttentionCheckQuestion.check.correctAnswer.ToUpper() == "NO" ? "NO" : "ATTENTION_FAIL";
        LogAttentionResponse(response);
        EndAttentionCheck();
    }

    private void OnAttentionCheckRight(InputAction.CallbackContext ctx)
    {
        if (!isAttentionCheckActive || hasRespondedAttentionCheck) return;

        hasRespondedAttentionCheck = true;

        string response = currentAttentionCheckQuestion.check.correctAnswer.ToUpper() == "YES" ? "YES" : "ATTENTION_FAIL";
        LogAttentionResponse(response);
        EndAttentionCheck();
    }

    private void LogAttentionResponse(string response)
    {
        float reactionTime = Time.realtimeSinceStartup - attentionCheckStartTimeRealtime;
        float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

        var lastResponse = participantData.responses[participantData.responses.Count - 1];
        lastResponse.attentionCheckResponse = response;
        lastResponse.attentionCheckReactionTime = reactionTime.ToString("F3");

        participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = reactionTime,
            globalTimestamp = globalTimestamp,
            label = $"[AttentionCheck]: Question {currentQuestionIndex} | TopicCode: {currentAttentionCheckQuestion.topicCode} | StatementCode: {currentAttentionCheckQuestion.statementCode} | Response: {response}"
        });

        Debug.Log($"[AttentionCheck] Question {currentQuestionIndex}: {response} | LocalTimestamp: {reactionTime:F3}s | GlobalTimestamp: {globalTimestamp:F3}s");

        // ✅ Send LSL marker for attention check response
        LSLManager.Instance.SendMarker($"ATTENTION_CHECK_RESPONSE_Q{currentQuestionIndex}_{currentAttentionCheckQuestion.topicCode}_{currentAttentionCheckQuestion.statementCode}_{response}");
    }

    private void EndAttentionCheck()
    {
        isAttentionCheckActive = false;
        if (attentionCheckText != null) attentionCheckText.gameObject.SetActive(false);

        tempSelectedOption = "";
        currentQuestionIndex++;
        ProceedToNext();
    }

    private void ProceedToNext()
    {
        if (currentQuestionIndex < questions.Count && currentQuestionIndex % 20 == 0)
        {
            StartRestBreak();
        }
        else if (currentQuestionIndex >= questions.Count)
        {
            Debug.Log("[QuestionScene]: All questions completed!");
            StartCoroutine(TransitionToB4ArticleInstructionsScene());
        }
        else
        {
            StartCoroutine(TransitionToNextQuestion());
        }
    }

    private IEnumerator<WaitForSeconds> QuestionTimer()
    {
        yield return new WaitForSeconds(20f);

        if (!hasRespondedQuestion && !isAttentionCheckActive)
        {
            var q = questions[currentQuestionIndex];

            Debug.Log($"[Question {currentQuestionIndex}] {q.topicCode}-{q.statementCode} | No response within 20 seconds.");

            float localTimestamp = Time.realtimeSinceStartup - questionStartTimeRealtime;
            float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

            participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"Question_{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_NO_RESPONSE"
            });

            participantData.responses.Add(new ResponseRecord
            {
                questionIndex = currentQuestionIndex,
                topicCode = q.topicCode,
                statementCode = q.statementCode,
                selectedOption = "NR",
                agreementReactionTime = "20.000",
                attentionCheckResponse = "NR",
                attentionCheckReactionTime = "5.000"
            });

            LSLManager.Instance.SendMarker(
                $"[QuestionScene]: Question {currentQuestionIndex} | TopicCode: {q.topicCode} | StatementCode: {q.statementCode} | RESPONSE: NO_RESPONSE"
            );

            currentQuestionIndex++;
            ProceedToNext();
        }
    }

    private void StartRestBreak()
    {
        isResting = true;
        restStartTimeRealtime = Time.realtimeSinceStartup;

        ExperimentTimer2.Instance.StartRest();

        restBreakOverlay.SetActive(true);
        restBreakMessageText.text = "Rest Break: Please take a short break.\n\nPress SPACE to continue.";

        foreach (var optionText in optionTexts)
            optionText.text = "";

        // ✅ Send LSL marker for rest break start
        LSLManager.Instance.SendMarker($"REST_BREAK_START_Q{currentQuestionIndex}");

        // ✅ Record in participantData
        participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = 0f,
            globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp(),
            label = $"REST_BREAK_START_Q{currentQuestionIndex}"
        });

        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    private void OnContinuePressed(InputAction.CallbackContext ctx)
    {
        if (!isResting) return;

        inputActions.UI.Continue.performed -= OnContinuePressed;

        float restEndTime = Time.realtimeSinceStartup;
        float restDuration = restEndTime - restStartTimeRealtime;

        isResting = false;

        ExperimentTimer2.Instance.EndRest();

        questionStartTimeRealtime += restDuration;

        restBreakOverlay.SetActive(false);

        // ✅ Send LSL marker for rest break end
        LSLManager.Instance.SendMarker($"REST_BREAK_END_Q{currentQuestionIndex}");

        // ✅ Record in participantData
        participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = restDuration,
            globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp(),
            label = $"REST_BREAK_END_Q{currentQuestionIndex}"
        });

        LoadQuestion(currentQuestionIndex);
    }

    private IEnumerator<WaitForSeconds> TransitionToNextQuestion()
    {
        blankOverlay.SetActive(true);
        if (transitionXText != null) transitionXText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        blankOverlay.SetActive(false);
        if (transitionXText != null) transitionXText.gameObject.SetActive(false);

        LoadQuestion(currentQuestionIndex);
    }

    private IEnumerator<WaitForSeconds> TransitionToB4ArticleInstructionsScene()
    {
        blankOverlay.SetActive(true);
        if (transitionXText != null) transitionXText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("B4ArticleInstructionsScene");
    }
}