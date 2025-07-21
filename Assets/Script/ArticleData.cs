using System;
using System.Collections.Generic;

[Serializable]
public class Article
{
    public string headline;
    public string summary;
    public string content;
}

[Serializable]
public class ArticleList
{
    public string topic;
    public List<Article> articles;
}