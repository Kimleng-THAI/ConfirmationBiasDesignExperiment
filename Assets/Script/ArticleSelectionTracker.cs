using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ArticleSelectionTracker : MonoBehaviour
{
    public static ArticleSelectionTracker Instance;
    public int readArticleClickCount = 0;

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

    public void AddSelectedArticle(string topic, string headline, string content, string attentionWord, string attentionAnswer, string articleCode, string linkedStatement)
    {
        // Avoid duplicate entries
        if (!selectedArticles.articles.Any(a => a.topic == topic && a.headline == headline))
        {
            SelectedArticle newArticle = new SelectedArticle
            {
                topic = topic,
                headline = headline,
                content = content,
                attentionWord = attentionWord,
                attentionAnswer = attentionAnswer,
                articleCode = articleCode,
                linkedStatement = linkedStatement
            };

            selectedArticles.articles.Add(newArticle);

            //LSLManager.Instance.SendMarker(
            //    $"ARTICLE_READ_{newArticle.articleCode}_{newArticle.linkedStatement}_{newArticle.topic}_{newArticle.headline}"
            //);
        }
        // Count total clicks regardless of uniqueness
        readArticleClickCount++;
    }

    public SelectedArticleList GetSelectedArticles()
    {
        return selectedArticles;
    }

    // Helper: Returns count of unique articles for a given topic
    public int GetUniqueArticleCountForTopic(string topic)
    {
        return selectedArticles.articles
            .Where(a => a.topic == topic)
            .Select(a => a.headline)
            .Distinct()
            .Count();
    }

    // Helper: Returns true if participant has read at least 2 unique articles per topic
    public bool HasReadMinimumTwoArticlesPerTopic(List<string> requiredTopics)
    {
        foreach (string topic in requiredTopics)
        {
            if (GetUniqueArticleCountForTopic(topic) < 2)
                return false;
        }
        return true;
    }

    // Returns true if participant has read at least 2 articles in at least 5 topics
    public bool HasReadMinimumPerFiveTopics(int minArticlesPerTopic = 2, int minTopics = 5)
    {
        int topicsMeetingRequirement = selectedArticles.articles
            .GroupBy(a => a.topic)
            .Count(g => g.Select(a => a.headline).Distinct().Count() >= minArticlesPerTopic);

        return topicsMeetingRequirement >= minTopics;
    }

    // Returns total number of unique articles read
    public int GetTotalUniqueArticlesRead()
    {
        return selectedArticles.articles
            .Select(a => a.headline)
            .Distinct()
            .Count();
    }

}