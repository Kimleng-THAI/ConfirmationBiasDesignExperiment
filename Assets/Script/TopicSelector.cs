using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using LSL;

public class TopicSelector : MonoBehaviour
{
    public TMP_Dropdown topicDropdown;
    public Button continueButton;

    // Timestamp management
    private float sceneStartTime;          // Time when TopicSelector scene first appeared
    private float lastDropdownSelectTime;  // Time of last dropdown selection
    private float continueButtonTime;      // Time when continue button pressed

    private List<string> topics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Technology and Social Media Impact",
        "Economic Policy and Inequality",
        "Health and Medical Approaches",
        "Education and Learning Methods",
        "Artificial Intelligence and Ethics",
        "Work-Life Balance and Productivity",
        "Media and Information",
        "Science and Research Funding",
        "Parenting and Child Development",
        "Aging and Elder Care",
        "Mental Health and Wellness"
    };

    private void Start()
    {
        // Ensure sceneStartTime persists for rest/revisit consistency
        if (sceneStartTime == 0f)
            sceneStartTime = Time.realtimeSinceStartup;

        // Initialize lastDropdownSelectTime to sceneStartTime
        // so first selection is measured correctly
        lastDropdownSelectTime = sceneStartTime;

        SetupDropdownOptions();
        continueButton.interactable = false;

        topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void SetupDropdownOptions()
    {
        topicDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Choose Topic...")
        };

        var tracker = ArticleSelectionTracker.Instance;

        foreach (string topic in topics)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData(topic);

            if (tracker != null && tracker.GetUniqueArticleCountForTopic(topic) >= 2)
                option.text += " (completed)";

            dropdownOptions.Add(option);
        }

        topicDropdown.AddOptions(dropdownOptions);
        topicDropdown.value = 0;
        topicDropdown.captionText.text = "Choose Topic...";
    }

    private void OnDropdownValueChanged(int index)
    {
        // Enable the continue button only if a valid topic is selected
        continueButton.interactable = index > 0;

        if (index > 0)
        {
            string selectedTopic = topicDropdown.options[index].text.Replace(" (completed)", "");
            float currentTime = Time.realtimeSinceStartup;

            // Local timestamp: time since last selection or scene start
            //float localTimestamp = lastDropdownSelectTime > 0f ? currentTime - lastDropdownSelectTime : 0f;
            float localTimestamp = currentTime - lastDropdownSelectTime;
            float globalTimestamp = currentTime - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"TOPIC_SELECTED: {selectedTopic}"
            });

            Debug.Log($"[TopicSelector]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: TOPIC_SELECTED: {selectedTopic}");
            LSLManager.Instance.SendMarker($"TOPIC_SELECT_{selectedTopic}");

            // Update the last selection timestamp
            lastDropdownSelectTime = currentTime;
        }
    }

    private void OnContinueButtonClicked()
    {
        if (topicDropdown.value == 0)
        {
            Debug.LogWarning("[TopicSelector]: Cannot continue. No valid topic selected.");
            return;
        }

        continueButtonTime = Time.realtimeSinceStartup;

        // Calculate local timestamp relative to dropdown selection
        float localTimestamp = continueButtonTime - lastDropdownSelectTime;
        float globalTimestamp = continueButtonTime - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = "[TopicSelector]: CONTINUE_BUTTON_CLICKED"
        });

        Debug.Log($"[TopicSelector]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: CONTINUE_BUTTON_CLICKED");
        LSLManager.Instance.SendMarker("TOPIC_SELECTOR_CONTINUE");

        string selectedTopic = topicDropdown.options[topicDropdown.value].text.Replace(" (completed)", "");
        Debug.Log("[TopicSelector]: Topic selected - " + selectedTopic);
        // Save selected topic to PlayerPrefs
        PlayerPrefs.SetString("SelectedTopic", selectedTopic);
        PlayerPrefs.Save();

        // Load next scene
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleSelectorScene");
        SceneManager.LoadScene("TransitionScene");
    }

    public void AdjustSceneStartTimeForRest(float restDuration)
    {
        sceneStartTime += restDuration;
        Debug.Log($"[TopicSelector]: sceneStartTime adjusted by {restDuration:F3}s after rest. New sceneStartTime: {sceneStartTime:F3}s");
    }
}