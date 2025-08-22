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
}

[Serializable]
public class SelectedArticleList
{
    public List<SelectedArticle> articles = new List<SelectedArticle>();
}