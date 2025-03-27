using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Repository;

namespace PhienNTMVC.Pages.categories
{
    public class DetailsModel : PageModel
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly INewsArticleRepo _newsArticleRepo;
        private readonly IConfiguration _configuration;

        public DetailsModel(
            ICategoryRepo categoryRepo,
            INewsArticleRepo newsArticleRepo,
            IConfiguration configuration)
        {
            _categoryRepo = categoryRepo;
            _newsArticleRepo = newsArticleRepo;
            _configuration = configuration;
        }

        public Category Category { get; set; } = default!;
        public IEnumerable<NewsArticle> ArticlesInCategory { get; set; }
        public bool HasSubcategories { get; set; }
        public IEnumerable<Category> Subcategories { get; set; }

        public IActionResult OnGet(int? id)
        {
            // Check if user is logged in and is staff
            var userEmail = HttpContext.Session.GetString("email");
            var userRole = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            // Check if user is staff
            if (userRole != "Staff" && userRole != _configuration["StaffRole:RoleId"])
            {
                return RedirectToPage("/Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = _categoryRepo.GetCategoryById(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            Category = category;

            // Get articles in this category
            ArticlesInCategory = _newsArticleRepo.GetNewsArticlesByCategory(id.Value);

            // Get subcategories
            Subcategories = _categoryRepo.GetAllCategories()
                .Where(c => c.ParentCategoryId == id.Value)
                .ToList();

            HasSubcategories = Subcategories.Any();

            return Page();
        }
    }
}