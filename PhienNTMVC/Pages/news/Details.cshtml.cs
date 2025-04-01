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
    public class DetailsModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly ITagRepo _tagRepo;
        private readonly IConfiguration _configuration;

        public DetailsModel(
            INewsArticleRepo newsRepo,
            ITagRepo tagRepo,
            IConfiguration configuration)
        {
            _newsRepo = newsRepo;
            _tagRepo = tagRepo;
            _configuration = configuration;
        }

        public NewsArticle NewsArticle { get; set; }
        public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();
        public string ErrorMessage { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get the news article
                NewsArticle = _newsRepo.GetNewsArticleById(id.Value);

                if (NewsArticle == null)
                {
                    return NotFound();
                }

                // Check authentication status
                bool isAuthenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("email"));
                string userRole = HttpContext.Session.GetString("role");

                // For non-authenticated users, only show published articles
                if (!isAuthenticated && NewsArticle.NewsStatus != "Published")
                {
                    ErrorMessage = "This article is not available for public viewing.";
                    return RedirectToPage("/Index");
                }

                // For lecturer role, only show published articles
                bool isLecturer = userRole == "Lecturer" || userRole == _configuration["LecturerRole:RoleId"];
                if (isLecturer && NewsArticle.NewsStatus != "Published")
                {
                    ErrorMessage = "This article is not published yet.";
                    return RedirectToPage("/news/Index");
                }

                // Get the tags for this article
                try
                {
                    Tags = _tagRepo.GetTagsByArticleId(id.Value) ?? new List<Tag>();
                }
                catch (Exception ex)
                {
                    // If we fail to get tags, at least show the article
                    Console.WriteLine($"Error retrieving tags: {ex.Message}");
                    Tags = new List<Tag>();
                }

                return Page();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Details: {ex.Message}");
                ErrorMessage = "An error occurred while retrieving the article.";
                return RedirectToPage("/news/Index");
            }
        }
    }
}