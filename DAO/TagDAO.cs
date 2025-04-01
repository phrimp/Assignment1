using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class TagDAO
    {
        private NewsSystemContext _dbContext;
        private static TagDAO instance;

        public TagDAO()
        {
            _dbContext = new NewsSystemContext();
        }

        public static TagDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TagDAO();
                }
                return instance;
            }
        }

        public IEnumerable<Tag> GetAllTags()
        {
            return _dbContext.Tags.ToList();
        }

        public Tag GetTagById(int id)
        {
            return _dbContext.Tags.FirstOrDefault(t => t.TagId == id);
        }

        public Tag GetTagByName(string name)
        {
            return _dbContext.Tags.FirstOrDefault(t => t.TagName == name);
        }

        public IEnumerable<Tag> SearchTags(string searchTerm)
        {
            return _dbContext.Tags
                .Where(t => t.TagName.Contains(searchTerm) ||
                            (t.Note != null && t.Note.Contains(searchTerm)))
                .ToList();
        }

        public void AddTag(Tag tag)
        {
            _dbContext.Tags.Add(tag);
            _dbContext.SaveChanges();
        }

        public void UpdateTag(Tag tag)
        {
            _dbContext.Entry(tag).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void DeleteTag(int id)
        {
            var tag = _dbContext.Tags.Find(id);
            if (tag != null)
            {
                _dbContext.Tags.Remove(tag);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<Tag> GetTagsByArticleId(int articleId)
        {
            return _dbContext.NewsArticles
                .Include(a => a.Tags)
                .FirstOrDefault(a => a.NewsArticleId == articleId)?.Tags;
        }

        public bool IsTagUsedInArticles(int tagId)
        {
            return _dbContext.Tags
                .Include(t => t.NewsArticles)
                .FirstOrDefault(t => t.TagId == tagId)?.NewsArticles.Any() ?? false;
        }
        // This should be in your NewsArticleDAO.cs file

        public void AddTagToNewsArticle(int articleId, int tagId)
        {
            try
            {
                // Always use a fresh context for relationship operations
                using (var context = new NewsSystemContext())
                {
                    var article = context.NewsArticles
                        .Include(a => a.Tags)
                        .FirstOrDefault(a => a.NewsArticleId == articleId);

                    var tag = context.Tags.Find(tagId);

                    if (article != null && tag != null)
                    {
                        // Check if the relationship already exists
                        if (!article.Tags.Any(t => t.TagId == tagId))
                        {
                            article.Tags.Add(tag);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding tag to article: {ex.Message}");
                throw;
            }
        }

        public void RemoveTagFromNewsArticle(int articleId, int tagId)
        {
            try
            {
                // Always use a fresh context for relationship operations
                using (var context = new NewsSystemContext())
                {
                    var article = context.NewsArticles
                        .Include(a => a.Tags)
                        .FirstOrDefault(a => a.NewsArticleId == articleId);

                    if (article != null)
                    {
                        var tagToRemove = article.Tags.FirstOrDefault(t => t.TagId == tagId);
                        if (tagToRemove != null)
                        {
                            article.Tags.Remove(tagToRemove);
                            context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing tag from article: {ex.Message}");
                throw;
            }
        }
    }
}