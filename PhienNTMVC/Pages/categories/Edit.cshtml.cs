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
    public class EditModel : PageModel
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly IConfiguration _configuration;

        public EditModel(ICategoryRepo categoryRepo, IConfiguration configuration)
        {
            _categoryRepo = categoryRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public SelectList ParentCategorySelectList { get; set; }

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

            // Get all categories for parent dropdown, excluding the current category
            var categories = _categoryRepo.GetAllCategories()
                .Where(c => c.CategoryId != id)
                .ToList();

            ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
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
                var categories = _categoryRepo.GetAllCategories()
                    .Where(c => c.CategoryId != Category.CategoryId)
                    .ToList();
                ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                return Page();
            }

            try
            {
                // Prevent circular reference by setting parent category to itself
                if (Category.ParentCategoryId == Category.CategoryId)
                {
                    ModelState.AddModelError("Category.ParentCategoryId", "A category cannot be its own parent.");
                    var categories = _categoryRepo.GetAllCategories()
                        .Where(c => c.CategoryId != Category.CategoryId)
                        .ToList();
                    ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                    return Page();
                }

                // Prevent circular references in the parent hierarchy
                if (Category.ParentCategoryId.HasValue)
                {
                    var parentId = Category.ParentCategoryId.Value;
                    var visited = new HashSet<int>();
                    visited.Add(Category.CategoryId);

                    while (parentId != 0)
                    {
                        if (visited.Contains(parentId))
                        {
                            ModelState.AddModelError("Category.ParentCategoryId", "Circular reference detected in parent category hierarchy.");
                            var categories = _categoryRepo.GetAllCategories()
                                .Where(c => c.CategoryId != Category.CategoryId)
                                .ToList();
                            ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                            return Page();
                        }

                        visited.Add(parentId);
                        var parent = _categoryRepo.GetCategoryById(parentId);
                        if (parent == null || !parent.ParentCategoryId.HasValue)
                            break;

                        parentId = parent.ParentCategoryId.Value;
                    }
                }

                _categoryRepo.UpdateCategory(Category);
                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating category: {ex.Message}");
                var categories = _categoryRepo.GetAllCategories()
                    .Where(c => c.CategoryId != Category.CategoryId)
                    .ToList();
                ParentCategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                return Page();
            }
        }
    }
}