using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TopicSelector : MonoBehaviour
{
    public TMP_Dropdown topicDropdown;
    public Button continueButton;

    private List<string> topics = new List<string>
    {
        "Climate Change and Environmental Policy",
        "Technology and Social Media Impact",
        "Economic Policy and Inequality",
        "Health and Medical Approaches",
        "Education and Learning Methods"
    };

    private void Start()
    {
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
    }

    private void OnContinueButtonClicked()
    {
        string selectedTopic = topicDropdown.options[topicDropdown.value].text;
        Debug.Log("[TopicSelector]: Topic selected - " + selectedTopic);

        // Save selected topic to PlayerPrefs
        PlayerPrefs.SetString("SelectedTopic", selectedTopic);
        PlayerPrefs.Save();

        // Load next scene
        SceneManager.LoadScene("ArticleSelectorScene");
    }
}