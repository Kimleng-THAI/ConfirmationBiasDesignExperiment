using System;
using System.Collections.Generic;

[Serializable]
public class SelectedArticle
{
    public string topic;
    public string headline;
    public string content;
}

[Serializable]
public class SelectedArticleList
{
    public List<SelectedArticle> articles = new List<SelectedArticle>();
}