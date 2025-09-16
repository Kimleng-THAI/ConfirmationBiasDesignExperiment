using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EventMarker
{
    // Time since current scene started
    public float localTimestamp;
    // Time since experiment started
    public float globalTimestamp;
    public string label;
}

[Serializable]
public class ParticipantData1
{
    public string subjectNumber;
    public int age;
    public string feedback;
    // ISO 8601 format
    public string experimentStartTime;
    public string experimentEndTime;
    public string duration;

    public string instructionScreenReactionTime;
    public string surveySceneDuration;
    public string thankYouSceneDuration;

    public List<ResponseRecord> responses = new List<ResponseRecord>();

    public int totalReadArticleClicks;
    public string selectedFinalTopic;
    //public string selectedArticleHeadline;
    //public string selectedArticleContent;
    public List<SelectedArticle> selectedArticles = new List<SelectedArticle>();

    public List<EventMarker> eventMarkers = new List<EventMarker>();
}

[Serializable]
public class ResponseRecord
{
    public int questionIndex;
    public string topicCode;
    public string statementCode;
    public string selectedOption;
    public string agreementReactionTime;
    public string attentionCheckResponse;
    public string attentionCheckReactionTime;
}
