using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class NewsArticleDAO
    {
        private NewsSystemContext _dbContext;
        private static NewsArticleDAO instance;

        public NewsArticleDAO()
        {
            _dbContext = new NewsSystemContext();
        }

        public static NewsArticleDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NewsArticleDAO();
                }
                return instance;
            }
        }

        public IEnumerable<NewsArticle> GetAllNewsArticles()
        {
            _dbContext.ChangeTracker.Clear();

            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .AsNoTracking() 
                .ToList();
        }

        public IEnumerable<NewsArticle> GetActiveNewsArticles()
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .Where(a => a.NewsStatus == "Published" && a.Category.IsActive)
                .ToList();
        }

        public NewsArticle GetNewsArticleById(int id)
        {
            try
            {
                return _dbContext.NewsArticles
                    .Include(a => a.Category)
                    .Include(a => a.CreatedBy)
                    .Include(a => a.UpdatedBy)
                    .Include(a => a.Tags)
                    .FirstOrDefault(a => a.NewsArticleId == id);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in GetNewsArticleById: {ex.Message}");
                throw; // Rethrow to let the caller handle it
            }
        }

        public IEnumerable<NewsArticle> GetNewsArticlesByCategory(int categoryId)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .Where(a => a.CategoryId == categoryId)
                .ToList();
        }

        public IEnumerable<NewsArticle> GetNewsArticlesByCreator(int creatorId)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .Where(a => a.CreatedById == creatorId)
                .ToList();
        }

        public IEnumerable<NewsArticle> SearchNewsArticles(string searchTerm)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .Where(a => a.NewsTitle.Contains(searchTerm) ||
                            a.Headline.Contains(searchTerm) ||
                            a.NewsContent.Contains(searchTerm))
                .ToList();
        }

        public IEnumerable<NewsArticle> GetNewsArticlesByDate(DateTime startDate, DateTime endDate)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Category)
                .Include(a => a.CreatedBy)
                .Include(a => a.UpdatedBy)
                .Include(a => a.Tags)
                .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate)
                .OrderByDescending(a => a.CreatedDate)
                .ToList();
        }

        public void AddNewsArticle(NewsArticle article)
        {
            _dbContext.NewsArticles.Add(article);
            _dbContext.SaveChanges();
        }

        public void UpdateNewsArticle(NewsArticle article)
        {
            try
            {
                using (var context = new NewsSystemContext())
                {
                    var existingArticle = context.NewsArticles.Find(article.NewsArticleId);

                    if (existingArticle == null)
                        throw new Exception("Article not found");

                    existingArticle.NewsTitle = article.NewsTitle;
                    existingArticle.Headline = article.Headline;
                    existingArticle.NewsContent = article.NewsContent;
                    existingArticle.NewsSource = article.NewsSource;
                    existingArticle.CategoryId = article.CategoryId;
                    existingArticle.NewsStatus = article.NewsStatus;
                    existingArticle.UpdatedById = article.UpdatedById;
                    existingArticle.ModifiedDate = article.ModifiedDate;

                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error updating article: {ex.Message}");
                throw; // Rethrow to let the caller handle it
            }
        }

        public void DeleteNewsArticle(int id)
        {
            var article = _dbContext.NewsArticles.Find(id);
            if (article != null)
            {
                _dbContext.NewsArticles.Remove(article);
                _dbContext.SaveChanges();
            }
        }

        public void AddTagToNewsArticle(int articleId, int tagId)
        {
            var article = _dbContext.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefault(a => a.NewsArticleId == articleId);

            var tag = _dbContext.Tags.Find(tagId);

            if (article != null && tag != null)
            {
                article.Tags.Add(tag);
                _dbContext.SaveChanges();
            }
        }

        public void RemoveTagFromNewsArticle(int articleId, int tagId)
        {
            var article = _dbContext.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefault(a => a.NewsArticleId == articleId);

            var tag = _dbContext.Tags.Find(tagId);

            if (article != null && tag != null && article.Tags.Contains(tag))
            {
                article.Tags.Remove(tag);
                _dbContext.SaveChanges();
            }
        }

    }
}