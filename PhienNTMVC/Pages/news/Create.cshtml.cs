using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Repository;

namespace PhienNTMVC.Pages.news
{
    public class CreateModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly ITagRepo _tagRepo;

        public CreateModel(
            INewsArticleRepo newsRepo,
            ICategoryRepo categoryRepo,
            IAccountRepo accountRepo,
            ITagRepo tagRepo)
        {
            _newsRepo = newsRepo;
            _categoryRepo = categoryRepo;
            _accountRepo = accountRepo;
            _tagRepo = tagRepo;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = new NewsArticle();

        [BindProperty]
        public string SelectedTags { get; set; }

        [BindProperty]
        public string NewTags { get; set; }

        public SelectList CategorySelectList { get; set; }
        public List<Tag> AvailableTags { get; set; } = new List<Tag>();

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            var currentUser = _accountRepo.GetAccountByEmail(userEmail);
            if (currentUser == null)
            {
                return RedirectToPage("/login");
            }

            NewsArticle.CreatedDate = DateTime.Now;
            NewsArticle.CreatedById = currentUser.AccountId;
            NewsArticle.NewsStatus = "Draft"; // Default status

            // Get active categories for dropdown
            var categories = _categoryRepo.GetActiveCategories().ToList();
            CategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");

            // Get all tags for selection
            AvailableTags = _tagRepo.GetAllTags().ToList();

            return Page();
        }

        public IActionResult OnPost()
        {
            var userEmail = HttpContext.Session.GetString("email");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/login");
            }

            var currentUser = _accountRepo.GetAccountByEmail(userEmail);
            if (currentUser == null)
            {
                return RedirectToPage("/login");
            }

            

            try
            {
                // Set creation metadata
                NewsArticle.CreatedDate = DateTime.Now;
                NewsArticle.CreatedById = currentUser.AccountId;

                if (string.IsNullOrEmpty(NewsArticle.NewsStatus))
                {
                    NewsArticle.NewsStatus = "Draft";
                }

                // Add the article first to get its ID
                _newsRepo.AddNewsArticle(NewsArticle);

                // Process selected existing tags
                if (!string.IsNullOrEmpty(SelectedTags))
                {
                    var tagIds = SelectedTags.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(int.Parse)
                        .ToList();

                    foreach (var tagId in tagIds)
                    {
                        _newsRepo.AddTagToNewsArticle(NewsArticle.NewsArticleId, tagId);
                    }
                }

                // Process new tags
                if (!string.IsNullOrEmpty(NewTags))
                {
                    var tagNames = NewTags.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();

                    foreach (var tagName in tagNames)
                    {
                        var existingTag = _tagRepo.GetTagByName(tagName);

                        if (existingTag == null)
                        {
                            // Create new tag
                            var newTag = new Tag
                            {
                                TagName = tagName
                            };
                            _tagRepo.AddTag(newTag);
                            existingTag = _tagRepo.GetTagByName(tagName);
                        }

                        // Add the tag to the article
                        if (existingTag != null)
                        {
                            _newsRepo.AddTagToNewsArticle(NewsArticle.NewsArticleId, existingTag.TagId);
                        }
                    }
                }

                TempData["SuccessMessage"] = "News article created successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating news article: {ex.Message}");
                var categories = _categoryRepo.GetActiveCategories().ToList();
                CategorySelectList = new SelectList(categories, "CategoryId", "CategoryName");
                AvailableTags = _tagRepo.GetAllTags().ToList();
                return Page();
            }
        }
    }
}