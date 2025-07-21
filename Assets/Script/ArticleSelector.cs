using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArticleSelector : MonoBehaviour
{
    // A Vertical Layout Group container
    public Transform contentParent;
    public GameObject articleCardPrefab;

    private List<Article> loadedArticles;

    void Start()
    {
        // change to dynamic in future if needed
        string topicFile = "Articles/climate_change";
        TextAsset jsonText = Resources.Load<TextAsset>(topicFile);

        if (jsonText != null)
        {
            ArticleList articleList = JsonUtility.FromJson<ArticleList>(jsonText.text);
            loadedArticles = articleList.articles;

            foreach (Article article in loadedArticles)
            {
                GameObject card = Instantiate(articleCardPrefab, contentParent);
                card.transform.Find("HeadlineText").GetComponent<TMP_Text>().text = article.headline;
                card.transform.Find("SummaryText").GetComponent<TMP_Text>().text = article.summary;

                Button readButton = card.transform.Find("ReadButton").GetComponent<Button>();
                readButton.onClick.AddListener(() =>
                {
                    ArticleManager.selectedArticle = article;
                    SceneManager.LoadScene("ArticleViewerScene");
                });
            }
        }
        else
        {
            Debug.LogError("Article JSON not found.");
        }
    }
}
