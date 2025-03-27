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

        public DeleteModel(INewsArticleRepo newsRepo, IAccountRepo accountRepo)
        {
            _newsRepo = newsRepo;
            _accountRepo = accountRepo;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; }

        public IActionResult OnGet(int? id)
        {
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            if (id == null)
            {
                return NotFound();
            }

            NewsArticle = _newsRepo.GetNewsArticleById(id.Value);

            if (NewsArticle == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            if (NewsArticle == null || NewsArticle.NewsArticleId == 0)
            {
                return NotFound();
            }

            try
            {
                _newsRepo.DeleteNewsArticle(NewsArticle.NewsArticleId);
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting news article: {ex.Message}");
                NewsArticle = _newsRepo.GetNewsArticleById(NewsArticle.NewsArticleId);
                return Page();
            }
        }
    }
}