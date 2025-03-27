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

        public DetailsModel(INewsArticleRepo newsRepo, ITagRepo tagRepo)
        {
            _newsRepo = newsRepo;
            _tagRepo = tagRepo;
        }

        public NewsArticle NewsArticle { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NewsArticle = _newsRepo.GetNewsArticleById(id.Value);

            if (NewsArticle == null)
            {
                return NotFound();
            }

            bool isAuthenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("email"));

            if (!isAuthenticated && NewsArticle.NewsStatus != "Published")
            {
                return RedirectToPage("/Index");
            }

            Tags = _tagRepo.GetTagsByArticleId(id.Value);

            return Page();
        }
    }
}