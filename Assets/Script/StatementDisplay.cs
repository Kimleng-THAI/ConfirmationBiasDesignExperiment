using TMPro;
using UnityEngine;

public class StatementDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statementText;

    void Start()
    {
        // Fetch the belief input from the ParticipantData class
        string participantBelief = ParticipantData.ParticipantBelief;

        // Display the participant's belief
        statementText.text = participantBelief;

        Debug.Log($"Displayed Statement: {participantBelief}");
    }
}
