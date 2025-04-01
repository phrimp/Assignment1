using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Repository;

namespace PhienNTMVC.Pages.news
{
    public class DeleteModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public DeleteModel(
            INewsArticleRepo newsRepo,
            IAccountRepo accountRepo,
            IConfiguration configuration)
        {
            _newsRepo = newsRepo;
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnGet(int? id)
        {
            // Check authentication
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            // Check if ID is provided
            if (id == null)
            {
                return NotFound();
            }

            // Get the news article with its related data
            NewsArticle = _newsRepo.GetNewsArticleById(id.Value);

            if (NewsArticle == null)
            {
                return NotFound();
            }

            // Check if current user has right to delete this article
            var userRole = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            // Only admin or the creator can delete the article
            bool isAdmin = userRole == "Admin" || userRole == _configuration["AdminRole:RoleId"];
            bool isCreator = userId.HasValue && userId.Value == NewsArticle.CreatedById;
            bool isStaff = userRole == "Staff" || userRole == _configuration["StaffRole:RoleId"];

            if (!isAdmin && !isCreator && !(isStaff && NewsArticle.CreatedById == userId))
            {
                ErrorMessage = "You don't have permission to delete this article.";
                return Page();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            // Check authentication
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            // Recheck permission - security in depth
            var userRole = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            // Get the article to check permissions and make sure it exists
            var article = _newsRepo.GetNewsArticleById(id);
            if (article == null)
            {
                return NotFound();
            }

            // Only admin or the creator can delete
            bool isAdmin = userRole == "Admin" || userRole == _configuration["AdminRole:RoleId"];
            bool isCreator = userId.HasValue && userId.Value == article.CreatedById;
            bool isStaff = userRole == "Staff" || userRole == _configuration["StaffRole:RoleId"];

            if (!isAdmin && !isCreator && !(isStaff && article.CreatedById == userId))
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this article.";
                return RedirectToPage("./Index");
            }

            try
            {
                _newsRepo.DeleteNewsArticle(id);
                TempData["SuccessMessage"] = "News article was deleted successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting news article: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}