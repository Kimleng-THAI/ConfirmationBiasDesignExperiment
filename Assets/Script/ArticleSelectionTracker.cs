using UnityEngine;
using System.Collections.Generic;

public class ArticleSelectionTracker : MonoBehaviour
{
    public static ArticleSelectionTracker Instance;

    public SelectedArticleList selectedArticles = new SelectedArticleList();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddSelectedArticle(string topic, string headline, string content)
    {
        SelectedArticle newArticle = new SelectedArticle
        {
            topic = topic,
            headline = headline,
            content = content
        };

        selectedArticles.articles.Add(newArticle);
    }

    public SelectedArticleList GetSelectedArticles()
    {
        return selectedArticles;
    }
}