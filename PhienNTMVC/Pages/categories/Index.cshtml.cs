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
    public class IndexModel : PageModel
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly IConfiguration _configuration;

        public IndexModel(ICategoryRepo categoryRepo, IConfiguration configuration)
        {
            _categoryRepo = categoryRepo;
            _configuration = configuration;
        }

        public IList<Category> Categories { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

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

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                Categories = _categoryRepo.SearchCategories(SearchTerm).ToList();
            }
            else
            {
                Categories = _categoryRepo.GetAllCategories().ToList();
            }

            return Page();
        }
    }
}