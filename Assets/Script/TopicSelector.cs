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

    private bool hasRemovedPlaceholder = false;

    private void Start()
    {
        // Insert "Choose Topic..." at the top
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("Choose Topic...")
        };

        foreach (string topic in topics)
        {
            dropdownOptions.Add(new TMP_Dropdown.OptionData(topic));
        }

        topicDropdown.options = dropdownOptions;
        topicDropdown.value = 0;
        topicDropdown.captionText.text = "Choose Topic...";

        continueButton.interactable = false;

        topicDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(); });

        // Remove the placeholder when dropdown is clicked
        topicDropdown.onValueChanged.AddListener(delegate { RemovePlaceholderIfNeeded(); });

        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    private void OnDropdownValueChanged()
    {
        // Only enable the continue button if a real topic is selected
        continueButton.interactable = topicDropdown.value != 0;
    }

    private void RemovePlaceholderIfNeeded()
    {
        if (hasRemovedPlaceholder) return;

        // Remove "Choose Topic..." from options
        topicDropdown.options.RemoveAt(0);
        topicDropdown.RefreshShownValue();  // Update the UI
        hasRemovedPlaceholder = true;
    }

    private void OnContinueButtonClicked()
    {
        string selectedTopic = topicDropdown.options[topicDropdown.value].text;
        Debug.Log("[TopicSelectorScene]: Topic selected - " + selectedTopic);
        SceneManager.LoadScene("SurveyScene");
    }
}