using System;
using System.Collections.Generic;

[Serializable]
public class SelectedArticle
{
    public string topic;
    public string headline;
    public string content;
    public string selectedOption;
    public string attentionWord;
    public string attentionAnswer;
    public string attentionCheckResponse;
    public string attentionCheckReactionTime;

    public string articleCode;
    public string linkedStatement;
}

[Serializable]
public class SelectedArticleList
{
    public List<SelectedArticle> articles = new List<SelectedArticle>();
}