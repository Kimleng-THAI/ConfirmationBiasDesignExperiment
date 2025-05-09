using TMPro;
using UnityEngine;

public class StatementDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statementText;

    // We can replace this with a dynamic value later
    private string participantBelief = "I believe that climate change is primarily caused by human activity.";

    void Start()
    {
        statementText.text = participantBelief;
        Debug.Log($"Displayed Statement: {participantBelief}");
    }
}
