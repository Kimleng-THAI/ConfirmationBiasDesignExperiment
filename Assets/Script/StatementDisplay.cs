using TMPro;
using UnityEngine;

public class StatementDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statementText;

    void Start()
    {
        // Access belief from the shared participantData instance
        string participantBelief = QuestionScreen.participantData.belief;

        // Display the participant's belief
        statementText.text = participantBelief;

        Debug.Log($"[StatementScene]: {participantBelief}");
    }
}
