using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class QuestionScreen : MonoBehaviour
{
    public GameObject blankOverlay;

    public TextMeshProUGUI conflictStatementText;
    public TextMeshProUGUI[] optionTexts;

    private PlayerInputActions inputActions;
    private static int currentQuestionIndex = 0;
    private List<Question> questions;

    private Coroutine eegCoroutine;
    public static float experimentStartTimeRealtime;

    // moved inside class
    public static ParticipantData1 participantData = new ParticipantData1();

    private float questionStartTimeRealtime;

    private Coroutine questionTimerCoroutine;
    private bool hasResponded = false;

    private bool awaitingConfidence = false;
    private float confidenceStartTime;
    private string tempSelectedOption = "";
    private string tempAgreementReactionTime = "";

    private bool isResting = false;
    private float restStartTimeRealtime;

    // The whole panel for rest break
    public GameObject restBreakOverlay;
    // The text inside the overlay
    public TextMeshProUGUI restBreakMessageText;

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

        experimentStartTimeRealtime = Time.realtimeSinceStartup;
        eegCoroutine = StartCoroutine(SimulatePhysiologicalData());
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
            StartCoroutine(TransitionToTopicSelectorScene());
            return;
        }

        // reset timer for each question
        questionStartTimeRealtime = Time.realtimeSinceStartup;

        // reset flags
        hasResponded = false;

        // stop any previous timer
        if (questionTimerCoroutine != null) StopCoroutine(questionTimerCoroutine);

        // start 10-second timer for non-response
        questionTimerCoroutine = StartCoroutine(QuestionTimer());

        var q = questions[index];
        conflictStatementText.text = $"<color=#000000><b>{q.topic}</b></color>\n\n{q.statement}";

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = (i < q.options.Count) ? q.options[i] : "";
        }
    }

    private void RecordResponse(string option)
    {
        // Ignore input while the subject is resting
        if (isResting) return;

        if (!awaitingConfidence)
        {
            // First stage — agreement selection
            if (hasResponded) return;
            hasResponded = true;

            // stop timer if answered early
            if (questionTimerCoroutine != null) StopCoroutine(questionTimerCoroutine);

            var q = questions[currentQuestionIndex];
            float rawReactionTime = Time.realtimeSinceStartup - questionStartTimeRealtime;
            tempAgreementReactionTime = rawReactionTime.ToString("F3");

            // Store agreement choice
            tempSelectedOption = option;
            confidenceStartTime = Time.realtimeSinceStartup;

            // Log agreement selection and reaction time
            Debug.Log($"[Question {currentQuestionIndex}] {q.topicCode}-{q.statementCode} | Agreement Selected Option: {tempSelectedOption} after {tempAgreementReactionTime} seconds.");

            // Log event marker for agreement scale rating
            float localTimestamp = Time.realtimeSinceStartup - questionStartTimeRealtime;
            //float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer2.Instance.ExperimentStartTimeRealtime2; Not Working
            float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

            participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"[QuestionScene]: Question: {currentQuestionIndex} | TopicCode: {q.topicCode} | StatementCode: {q.statementCode} | Agreement_Level: {option}"
            });

            Debug.Log($"[QuestionScene]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: Question_{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_AGREEMENT_{option}");

            // Switch to confidence mode
            awaitingConfidence = true;
            // allow input again
            hasResponded = false;
            ShowConfidenceOptions(q);
        }
        else
        {
            // Second stage — confidence selection
            var q = questions[currentQuestionIndex];
            float rawConfidenceReactionTime = Time.realtimeSinceStartup - confidenceStartTime;
            string confidenceReactionTime = rawConfidenceReactionTime.ToString("F3");

            participantData.responses.Add(new ResponseRecord
            {
                questionIndex = currentQuestionIndex,
                topicCode = q.topicCode,
                statementCode = q.statementCode,
                selectedOption = tempSelectedOption,
                confidenceLevel = option,
                agreementReactionTime = tempAgreementReactionTime,
                confidenceReactionTime = confidenceReactionTime
            });

            Debug.Log($"[Question {currentQuestionIndex}] {q.topicCode}-{q.statementCode} | Confidence Level: {option} after {confidenceReactionTime} seconds.");

            // Log event marker for confidence scale rating
            float localTimestamp = Time.realtimeSinceStartup - confidenceStartTime;
            float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

            participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"[QuestionScene]: Question: {currentQuestionIndex} | TopicCode: {q.topicCode} | StatementCode: {q.statementCode} | Confidence_Level: {option}"
            });

            Debug.Log($"[QuestionScene]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: Question_{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_CONFIDENCE_{option}");

            // Reset state
            awaitingConfidence = false;
            tempSelectedOption = "";
            tempAgreementReactionTime = "";

            currentQuestionIndex++;
            if (currentQuestionIndex < questions.Count && currentQuestionIndex % 20 == 0)
            {
                StartRestBreak();
            }
            else if (currentQuestionIndex >= questions.Count)
            {
                Debug.Log("[QuestionScene]: All questions completed!");
                StartCoroutine(TransitionToTopicSelectorScene());
            }
            else
            {
                StartCoroutine(TransitionToNextQuestion());
            }
        }
    }

    private void ShowConfidenceOptions(Question q)
    {
        conflictStatementText.text = "<color=#000000><b>How confident are you about your previous response?</b></color>";

        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = (i < q.confidenceOptions.Count) ? q.confidenceOptions[i] : "";
        }
    }

    private IEnumerator<WaitForSeconds> QuestionTimer()
    {
        yield return new WaitForSeconds(10f);

        if (!hasResponded)
        {
            var q = questions[currentQuestionIndex];

            Debug.Log($"[Question {currentQuestionIndex}] {q.topicCode}-{q.statementCode} | No response within 10 seconds.");

            // Log non-response event
            float localTimestamp = Time.realtimeSinceStartup - questionStartTimeRealtime;
            float globalTimestamp = ExperimentTimer2.Instance.GetGlobalTimestamp();

            participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"Question_{currentQuestionIndex}_{q.topicCode}_{q.statementCode}_NO_RESPONSE"
            });

            // Save non-response as "NR" for both agreement and confidence
            participantData.responses.Add(new ResponseRecord
            {
                questionIndex = currentQuestionIndex,
                topicCode = q.topicCode,
                statementCode = q.statementCode,
                selectedOption = "NR",
                confidenceLevel = "NR",
                agreementReactionTime = "10.000",
                confidenceReactionTime = "10.000"
            });

            currentQuestionIndex++;
            if (currentQuestionIndex < questions.Count && currentQuestionIndex % 20 == 0)
            {
                StartRestBreak();
            }
            else if (currentQuestionIndex >= questions.Count)
            {
                Debug.Log("[QuestionScene]: All questions completed!");
                StartCoroutine(TransitionToTopicSelectorScene());
            }
            else
            {
                StartCoroutine(TransitionToNextQuestion());
            }
        }
    }

    private void StartRestBreak()
    {
        isResting = true;
        restStartTimeRealtime = Time.realtimeSinceStartup;

        // Notify global timer rest started
        ExperimentTimer2.Instance.StartRest();

        restBreakOverlay.SetActive(true);
        restBreakMessageText.text = "Rest Break: Please take a short break.\n\nPress SPACE to continue.";

        foreach (var optionText in optionTexts)
            optionText.text = "";

        inputActions.UI.Continue.performed += OnContinuePressed;
    }

    private void OnContinuePressed(InputAction.CallbackContext ctx)
    {
        if (!isResting) return;

        inputActions.UI.Continue.performed -= OnContinuePressed;

        float restEndTime = Time.realtimeSinceStartup;
        float restDuration = restEndTime - restStartTimeRealtime;

        isResting = false;

        // Notify global timer rest ended
        ExperimentTimer2.Instance.EndRest();

        // Adjust question start time so local timestamp stays consistent
        questionStartTimeRealtime += restDuration;

        restBreakOverlay.SetActive(false);

        LoadQuestion(currentQuestionIndex);
    }

    private IEnumerator<WaitForSeconds> TransitionToNextQuestion()
    {
        // Show blank screen
        blankOverlay.SetActive(true);
        // Wait 1 second
        yield return new WaitForSeconds(1f);
        // Hide blank screen
        blankOverlay.SetActive(false);

        LoadQuestion(currentQuestionIndex);
    }

    private IEnumerator<WaitForSeconds> TransitionToTopicSelectorScene()
    {
        // Show blank screen
        blankOverlay.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("TopicSelectorScene");
    }

    private IEnumerator<WaitForSeconds> SimulatePhysiologicalData()
    {
        int heartRateStep = 0;

        while (true)
        {
            float timestamp = Time.realtimeSinceStartup - experimentStartTimeRealtime;

            // Simulate EEG every 0.1s
            float microvolts = Random.Range(10f, 100f);
            participantData.eegReadings.Add(new EEGReading
            {
                timestamp = timestamp,
                microvolts = microvolts
            });

            // Simulate heart rate every 10 steps (i.e., every 1s)
            if (heartRateStep % 10 == 0)
            {
                float bpm = Random.Range(60f, 100f);
                participantData.heartRateReadings.Add(new HeartRateReading
                {
                    // same timestamp
                    timestamp = timestamp,
                    bpm = bpm
                });
            }

            heartRateStep++;
            // 10Hz loop
            yield return new WaitForSeconds(0.1f);
        }
    }
}
