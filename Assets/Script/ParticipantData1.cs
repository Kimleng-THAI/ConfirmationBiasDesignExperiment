using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticipantData1
{
    public string studentID;
    public int age;
    public string feedback;

    public List<ResponseRecord> responses = new List<ResponseRecord>();
}

[Serializable]
public class ResponseRecord
{
    public int questionIndex;
    public string selectedOption;
    public float reactionTime;
}
