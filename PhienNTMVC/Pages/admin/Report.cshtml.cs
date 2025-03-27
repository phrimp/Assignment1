using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhienNTMVC.Pages.admin
{
    public class ReportsModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IConfiguration _configuration;

        public ReportsModel(
            INewsArticleRepo newsRepo,
            IAccountRepo accountRepo,
            ICategoryRepo categoryRepo,
            IConfiguration configuration)
        {
            _newsRepo = newsRepo;
            _accountRepo = accountRepo;
            _categoryRepo = categoryRepo;
            _configuration = configuration;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; } = DateTime.Now;

        public bool GeneratedReport { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<NewsArticle> NewsArticles { get; set; }
        public List<CategoryStat> ArticlesByCategory { get; set; } = new List<CategoryStat>();
        public List<StatusStat> ArticlesByStatus { get; set; } = new List<StatusStat>();
        public List<AuthorStat> ArticlesByAuthor { get; set; } = new List<AuthorStat>();

        public class CategoryStat
        {
            public string CategoryName { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public class StatusStat
        {
            public string Status { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public class AuthorStat
        {
            public string AuthorName { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        public IActionResult OnGet()
        {
            // Check if user is logged in and is admin
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            var currentUser = _accountRepo.GetAccountByEmail(userEmail);
            if (currentUser == null ||
                (currentUser.AccountRole != "Admin" &&
                 currentUser.AccountRole != _configuration["AdminRole:RoleId"]))
            {
                return RedirectToPage("/");
            }

            // Validate date range
            if (StartDate > EndDate)
            {
                ErrorMessage = "Start date cannot be after end date.";
                return Page();
            }

            if ((EndDate - StartDate).TotalDays > 365)
            {
                ErrorMessage = "Date range cannot be more than one year.";
                return Page();
            }

            // Check if this is a valid request for a report
            if (Request.Query.Count > 0)
            {
                // Get news articles in the date range
                try
                {
                    NewsArticles = _newsRepo.GetNewsArticlesByDate(StartDate, EndDate.AddDays(1));
                    GeneratedReport = true;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error generating report: {ex.Message}";
                    return Page();
                }

                // If no news articles found
                if (!NewsArticles.Any())
                {
                    ErrorMessage = "No news articles found in the selected date range.";
                    GeneratedReport = false;
                    return Page();
                }

                // Generate statistics
                GenerateStatistics();
            }

            return Page();
        }

        private void GenerateStatistics()
        {
            // Articles by category
            var categoryGroups = NewsArticles.GroupBy(a => a.CategoryId);
            var totalCount = NewsArticles.Count();

            foreach (var group in categoryGroups)
            {
                var category = _categoryRepo.GetCategoryById(group.Key);
                var count = group.Count();

                ArticlesByCategory.Add(new CategoryStat
                {
                    CategoryName = category.CategoryName,
                    Count = count,
                    Percentage = Math.Round((double)count / totalCount * 100, 1)
                });
            }

            // Articles by status
            var statusGroups = NewsArticles.GroupBy(a => a.NewsStatus);

            foreach (var group in statusGroups)
            {
                var count = group.Count();

                ArticlesByStatus.Add(new StatusStat
                {
                    Status = group.Key,
                    Count = count,
                    Percentage = Math.Round((double)count / totalCount * 100, 1)
                });
            }

            // Articles by author
            var authorGroups = NewsArticles.GroupBy(a => a.CreatedById);

            foreach (var group in authorGroups)
            {
                var author = _accountRepo.GetAccountById(group.Key);
                var count = group.Count();

                ArticlesByAuthor.Add(new AuthorStat
                {
                    AuthorName = author.AccountName,
                    Count = count,
                    Percentage = Math.Round((double)count / totalCount * 100, 1)
                });
            }
        }
    }
}