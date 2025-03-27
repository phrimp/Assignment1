using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Repository;

namespace PhienNTMVC.Pages.categories
{
    public class CreateModel : PageModel
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly IConfiguration _configuration;

        public CreateModel(ICategoryRepo categoryRepo, IConfiguration configuration)
        {
            _categoryRepo = categoryRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public SelectList ParentCategorySelectList { get; set; }

        public IActionResult OnGet()
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

            // Set default values
            Category = new Category
            {
                IsActive = true
            };

            // Get all categories for parent dropdown
            var categories = _categoryRepo.GetAllCategories().ToList();
            ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");

            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public IActionResult OnPost()
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

            if (!ModelState.IsValid || Category == null)
            {
                // Reload parent categories dropdown
                var categories = _categoryRepo.GetAllCategories().ToList();
                ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                return Page();
            }

            try
            {
                // Ensure we have a valid parent category ID or null
                if (Category.ParentCategoryId.HasValue)
                {
                    var parentCategory = _categoryRepo.GetCategoryById(Category.ParentCategoryId.Value);
                    if (parentCategory == null)
                    {
                        ModelState.AddModelError("Category.ParentCategoryId", "Invalid parent category selected.");
                        var categories = _categoryRepo.GetAllCategories().ToList();
                        ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                        return Page();
                    }
                }

                _categoryRepo.CreateCategory(Category);
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating category: {ex.Message}");
                var categories = _categoryRepo.GetAllCategories().ToList();
                ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                return Page();
            }
        }
    }
}