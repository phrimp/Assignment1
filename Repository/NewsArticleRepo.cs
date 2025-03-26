using DAO;
using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public class NewsArticleRepo : INewsArticleRepo
    {
        public IEnumerable<NewsArticle> GetAllNewsArticles() =>
            NewsArticleDAO.Instance.GetAllNewsArticles();

        public IEnumerable<NewsArticle> GetActiveNewsArticles() =>
            NewsArticleDAO.Instance.GetActiveNewsArticles();

        public NewsArticle GetNewsArticleById(int id) =>
            NewsArticleDAO.Instance.GetNewsArticleById(id);

        public IEnumerable<NewsArticle> GetNewsArticlesByCategory(int categoryId) =>
            NewsArticleDAO.Instance.GetNewsArticlesByCategory(categoryId);

        public IEnumerable<NewsArticle> GetNewsArticlesByCreator(int creatorId) =>
            NewsArticleDAO.Instance.GetNewsArticlesByCreator(creatorId);

        public IEnumerable<NewsArticle> SearchNewsArticles(string searchTerm) =>
            NewsArticleDAO.Instance.SearchNewsArticles(searchTerm);

        public IEnumerable<NewsArticle> GetNewsArticlesByDate(DateTime startDate, DateTime endDate) =>
            NewsArticleDAO.Instance.GetNewsArticlesByDate(startDate, endDate);

        public void AddNewsArticle(NewsArticle article) =>
            NewsArticleDAO.Instance.AddNewsArticle(article);

        public void UpdateNewsArticle(NewsArticle article) =>
            NewsArticleDAO.Instance.UpdateNewsArticle(article);

        public void DeleteNewsArticle(int id) =>
            NewsArticleDAO.Instance.DeleteNewsArticle(id);

        public void AddTagToNewsArticle(int articleId, int tagId) =>
            NewsArticleDAO.Instance.AddTagToNewsArticle(articleId, tagId);

        public void RemoveTagFromNewsArticle(int articleId, int tagId) =>
            NewsArticleDAO.Instance.RemoveTagFromNewsArticle(articleId, tagId);
    }
}