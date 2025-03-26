using DAO;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface ICategoryRepo
    {
        IEnumerable<Category> GetAllCategories();
        Category GetCategoryById(int id);
        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int id);
        IEnumerable<Category> GetActiveCategories();
        IEnumerable<Category> SearchCategories(string searchTerm);
        bool IsCategoryUsedInArticles(int categoryId);
    }
}
