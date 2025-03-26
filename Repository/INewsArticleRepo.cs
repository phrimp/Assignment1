using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public interface INewsArticleRepo
    {
        IEnumerable<NewsArticle> GetAllNewsArticles();
        IEnumerable<NewsArticle> GetActiveNewsArticles();
        NewsArticle GetNewsArticleById(int id);
        IEnumerable<NewsArticle> GetNewsArticlesByCategory(int categoryId);
        IEnumerable<NewsArticle> GetNewsArticlesByCreator(int creatorId);
        IEnumerable<NewsArticle> SearchNewsArticles(string searchTerm);
        IEnumerable<NewsArticle> GetNewsArticlesByDate(DateTime startDate, DateTime endDate);
        void AddNewsArticle(NewsArticle article);
        void UpdateNewsArticle(NewsArticle article);
        void DeleteNewsArticle(int id);
        void AddTagToNewsArticle(int articleId, int tagId);
        void RemoveTagFromNewsArticle(int articleId, int tagId);
    }
}