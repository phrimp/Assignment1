using DAO;
using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public class CategoryRepo : ICategoryRepo
    {
        public IEnumerable<Category> GetAllCategories() => CategoryDAO.Instance.GetAllCategories();

        public Category GetCategoryById(int id) => CategoryDAO.Instance.GetCategoryById(id);

        public void CreateCategory(Category category) => CategoryDAO.Instance.AddCategory(category);

        public void UpdateCategory(Category category) => CategoryDAO.Instance.UpdateCategory(category);

        public void DeleteCategory(int id) => CategoryDAO.Instance.DeleteCategory(id);

        public IEnumerable<Category> GetActiveCategories() => CategoryDAO.Instance.GetActiveCategories();

        public IEnumerable<Category> SearchCategories(string searchTerm) =>
            CategoryDAO.Instance.SearchCategories(searchTerm);

        public bool IsCategoryUsedInArticles(int categoryId) =>
            CategoryDAO.Instance.IsCategoryUsedInArticles(categoryId);
    }
}