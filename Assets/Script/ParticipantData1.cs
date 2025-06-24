using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParticipantData1
{
    public string studentID;
    public int age;
    public string feedback;

    public string belief;

    // ISO 8601 format
    public string experimentStartTime;
    public string experimentEndTime;

    public List<ResponseRecord> responses = new List<ResponseRecord>();
}

[Serializable]
public class ResponseRecord
{
    public int questionIndex;
    public string selectedOption;
    public float reactionTime;
}
