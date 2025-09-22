using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ArticleInstructionsManager : MonoBehaviour
{
    [Header("UI")]
    public Image instructionImage;          // Drag UI Image here in Inspector
    public TextMeshProUGUI captionText;     // Drag TMP Text here in Inspector

    private float sceneStartTime;
    private PlayerInputActions inputActions;

    private List<Sprite> instructionScreens = new List<Sprite>();
    private List<string> captions = new List<string>();
    private int currentIndex = 0;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        // Load screenshots from Resources/ArticleInstructions
        for (int i = 1; i <= 5; i++)
        {
            Sprite s = Resources.Load<Sprite>($"ArticleInstructions/N{i}");
            if (s != null)
            {
                instructionScreens.Add(s);
                captions.Add("Select a topic from the dropdown, and click Continue button.");
                captions.Add("Scroll down and choose an article.");
                captions.Add("Read the article carefully.");
                captions.Add("Provide us your rating (e.g., Press 1 for Strong Misalignment, or Press 5 for Strong Alignment).");
                captions.Add("Answer the short quiz.\n\n Press Spacebar to confirm you understand the demonstration screenshot.");
            }
        }

        if (instructionScreens.Count > 0)
        {
            currentIndex = 0;
            UpdateUI();
        }
        else
        {
            Debug.LogError("No instruction screenshots found in Resources/ArticleInstructions/");
        }
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

    private void OnContinuePressed(InputAction.CallbackContext ctx) => ShowNext();

    public void OnContinuePressed() => ShowNext();

    private void ShowNext()
    {
        if (instructionScreens.Count == 0) return;

        currentIndex++;

        if (currentIndex < instructionScreens.Count)
        {
            UpdateUI();
        }
        else
        {
            // Finished all instructions
            LogEvent("ArticleInstructions_FINISHED");
            PlayerPrefs.SetString("NextSceneAfterTransition", "TopicSelectorScene");
            //SceneManager.LoadScene("TransitionScene");
        }
    }

    private void UpdateUI()
    {
        instructionImage.sprite = instructionScreens[currentIndex];
        if (captionText != null && currentIndex < captions.Count)
        {
            captionText.text = captions[currentIndex];
        }
    }

    private void LogEvent(string label)
    {
        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = 0f;

        if (ExperimentTimer.Instance != null)
            globalTimestamp = ExperimentTimer.Instance.GetGlobalTimestamp();

        if (QuestionScreen.participantData != null)
        {
            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"{label} (local: {localTimestamp:F2}s)"
            });
        }

        Debug.Log($"[ArticleInstructions] {label} — Local: {localTimestamp:F2}s | Global: {globalTimestamp:F2}s");
    }
}