using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TopicSelector : MonoBehaviour
{
    public TMP_Dropdown topicDropdown;
    public Button continueButton;

    private float sceneStartTime;

    private List<string> topics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Technology and Social Media Impact",
        "Economic Policy and Inequality",
        "Health and Medical Approaches",
        "Education and Learning Methods",
        "Artificial Intelligence and Ethics",
        "Work-Life Balance and Productivity",
        "Urban Planning and Housing",
        "Food Systems and Agriculture",
        "Criminal Justice and Rehabilitation",
        "Gender and Society",
        "Immigration and Cultural Integration",
        "Privacy and Surveillance",
        "Sports and Competition",
        "Media and Information",
        "Science and Research Funding",
        "Parenting and Child Development",
        "Aging and Elder Care",
        "Transportation and Mobility",
        "Mental Health and Wellness"
    };

    private void Start()
    {
        sceneStartTime = Time.realtimeSinceStartup;

        // Clear existing options and add "Choose Topic..." placeholder
        topicDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Choose Topic...")
        };

        foreach (string topic in topics)
        {
            dropdownOptions.Add(new TMP_Dropdown.OptionData(topic));
        }

        topicDropdown.AddOptions(dropdownOptions);
        topicDropdown.value = 0;
        topicDropdown.captionText.text = "Choose Topic...";
        continueButton.interactable = false;

        topicDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnDropdownValueChanged(int index)
    {
        // Enable the continue button only if a valid topic is selected
        continueButton.interactable = index > 0;

        if (index > 0)
        {
            string selectedTopic = topicDropdown.options[index].text;
            float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
            float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

            QuestionScreen.participantData.eventMarkers.Add(new EventMarker
            {
                localTimestamp = localTimestamp,
                globalTimestamp = globalTimestamp,
                label = $"TOPIC_SELECTED: {selectedTopic}"
            });

            Debug.Log($"[TopicSelector]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: TOPIC_SELECTED: {selectedTopic}");
        }
    }

    private void OnContinueButtonClicked()
    {
        string selectedTopic = topicDropdown.options[topicDropdown.value].text;
        Debug.Log("[TopicSelector]: Topic selected - " + selectedTopic);

        float localTimestamp = Time.realtimeSinceStartup - sceneStartTime;
        float globalTimestamp = Time.realtimeSinceStartup - ExperimentTimer.Instance.ExperimentStartTimeRealtime;

        QuestionScreen.participantData.eventMarkers.Add(new EventMarker
        {
            localTimestamp = localTimestamp,
            globalTimestamp = globalTimestamp,
            label = "[TopicSelector]: CONTINUE_BUTTON_CLICKED"
        });

        Debug.Log($"[TopicSelector]: Event marker logged — Local: {localTimestamp:F3}s | Global: {globalTimestamp:F3}s | Label: CONTINUE_BUTTON_CLICKED");

        // Save selected topic to PlayerPrefs
        PlayerPrefs.SetString("SelectedTopic", selectedTopic);
        PlayerPrefs.Save();

        // Load next scene
        PlayerPrefs.SetString("NextSceneAfterTransition", "ArticleInstructionsScene");
        SceneManager.LoadScene("TransitionScene");
    }
}