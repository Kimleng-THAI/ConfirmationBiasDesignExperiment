using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Question
{
    public string topicCode;
    public string statementCode;
    public string topic;
    public string statement;
    public List<string> options;
    public List<string> confidenceOptions;
}

[System.Serializable]
public class QuestionList
{
    public List<Question> questions;
}

public class QuestionDataLoader : MonoBehaviour
{
    public static QuestionList LoadQuestionsFromJSON()
    {
        // Assets/Resources/questions.json
        TextAsset file = Resources.Load<TextAsset>("questions");
        if (file == null)
        {
            Debug.LogError("questions.json not found in Resources folder.");
            return null;
        }

        return JsonUtility.FromJson<QuestionList>(file.text);
    }
}
