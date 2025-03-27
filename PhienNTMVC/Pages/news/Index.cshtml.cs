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
    public class IndexModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly ICategoryRepo _categoryRepo;

        public IndexModel(INewsArticleRepo newsRepo, ICategoryRepo categoryRepo)
        {
            _newsRepo = newsRepo;
            _categoryRepo = categoryRepo;
        }

        public IList<NewsArticle> NewsArticle { get; set; } = new List<NewsArticle>();
        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public void OnGet()
        {
            Categories = _categoryRepo.GetActiveCategories().ToList();

            bool isAuthenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("email"));

            if (!isAuthenticated)
            {
                NewsArticle = _newsRepo.GetActiveNewsArticles().ToList();
            }
            else
            {
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    NewsArticle = _newsRepo.SearchNewsArticles(SearchTerm).ToList();
                }
                else if (CategoryId.HasValue)
                {
                    NewsArticle = _newsRepo.GetNewsArticlesByCategory(CategoryId.Value).ToList();
                }
                else
                {
                    NewsArticle = _newsRepo.GetAllNewsArticles().ToList();
                }
            }
        }
    }
}