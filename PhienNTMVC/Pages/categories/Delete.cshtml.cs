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
    public class DeleteModel : PageModel
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly IConfiguration _configuration;

        public DeleteModel(ICategoryRepo categoryRepo, IConfiguration configuration)
        {
            _categoryRepo = categoryRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public bool DeleteDisabled { get; set; }
        public string ErrorMessage { get; set; }

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

            // Check if the category is used in any news articles
            if (_categoryRepo.IsCategoryUsedInArticles(id.Value))
            {
                ErrorMessage = "This category cannot be deleted because it is used in one or more news articles.";
                DeleteDisabled = true;
            }

            // Check if the category has subcategories
            var hasSubcategories = _categoryRepo.GetAllCategories()
                .Any(c => c.ParentCategoryId == id);

            if (hasSubcategories)
            {
                ErrorMessage = "This category cannot be deleted because it has subcategories. Please remove or reassign the subcategories first.";
                DeleteDisabled = true;
            }

            return Page();
        }

        public IActionResult OnPost(int? id)
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

            // Check if the category is used in any news articles
            if (_categoryRepo.IsCategoryUsedInArticles(id.Value))
            {
                TempData["ErrorMessage"] = "This category cannot be deleted because it is used in one or more news articles.";
                return RedirectToPage("./Index");
            }

            // Check if the category has subcategories
            var hasSubcategories = _categoryRepo.GetAllCategories()
                .Any(c => c.ParentCategoryId == id);

            if (hasSubcategories)
            {
                TempData["ErrorMessage"] = "This category cannot be deleted because it has subcategories. Please remove or reassign the subcategories first.";
                return RedirectToPage("./Index");
            }

            try
            {
                _categoryRepo.DeleteCategory(id.Value);
                TempData["SuccessMessage"] = $"Category '{category.CategoryName}' was successfully deleted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}