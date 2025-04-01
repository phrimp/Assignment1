using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DAO
{
    public class CategoryDAO
    {
        private NewsSystemContext _dbContext;
        private static CategoryDAO instance;

        public CategoryDAO()
        {
            _dbContext = new NewsSystemContext();
        }

        public static CategoryDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CategoryDAO();
                }
                return instance;
            }
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return _dbContext.Categories.ToList();
        }

        public Category GetCategoryById(int id)
        {
            return _dbContext.Categories.FirstOrDefault(c => c.CategoryId == id);
        }

        public void AddCategory(Category category)
        {
            _dbContext.Categories.Add(category);
            _dbContext.SaveChanges();
        }

        public void UpdateCategory(Category category)
        {
            try
            {
                var existingEntry = _dbContext.ChangeTracker.Entries<Category>()
                    .FirstOrDefault(e => e.Entity.CategoryId == category.CategoryId);

                if (existingEntry != null)
                {
                    _dbContext.Entry(existingEntry.Entity).State = EntityState.Detached;
                }

                _dbContext.Attach(category);
                _dbContext.Entry(category).State = EntityState.Modified;

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                throw;
            }
        }

        public void DeleteCategory(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<Category> GetActiveCategories()
        {
            return _dbContext.Categories.Where(c => c.IsActive).ToList();
        }

        public IEnumerable<Category> SearchCategories(string searchTerm)
        {
            return _dbContext.Categories
                .Where(c => c.CategoryName.Contains(searchTerm) ||
                            c.CategoryDesciption.Contains(searchTerm))
                .ToList();
        }

        public bool IsCategoryUsedInArticles(int categoryId)
        {
            return _dbContext.NewsArticles.Any(a => a.CategoryId == categoryId);
        }
    }
}