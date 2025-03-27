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
    public class EditModel : PageModel
    {
        private readonly INewsArticleRepo _newsRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly IAccountRepo _accountRepo;
        private readonly ITagRepo _tagRepo;

        public EditModel(
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
        public NewsArticle NewsArticle { get; set; }

        [BindProperty]
        public string SelectedTags { get; set; } // Comma-separated tag ids

        [BindProperty]
        public string NewTags { get; set; } // Comma-separated new tag names

        public SelectList CategorySelectList { get; set; }
        public List<Tag> AvailableTags { get; set; } = new List<Tag>();
        public List<Tag> CurrentTags { get; set; } = new List<Tag>();

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

            var currentUser = _accountRepo.GetAccountByEmail(userEmail);
            if (currentUser == null)
            {
                return RedirectToPage("/login");
            }

            CategorySelectList = new SelectList(
                _categoryRepo.GetActiveCategories(),
                "CategoryId",
                "CategoryName");

            AvailableTags = _tagRepo.GetAllTags().ToList();

            CurrentTags = _tagRepo.GetTagsByArticleId(id.Value).ToList();

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

            if (!ModelState.IsValid)
            {
                CategorySelectList = new SelectList(
                    _categoryRepo.GetActiveCategories(),
                    "CategoryId",
                    "CategoryName");
                AvailableTags = _tagRepo.GetAllTags().ToList();
                CurrentTags = _tagRepo.GetTagsByArticleId(NewsArticle.NewsArticleId).ToList();
                return Page();
            }

            NewsArticle.UpdatedById = currentUser.AccountId;
            NewsArticle.ModifiedDate = DateTime.Now;

            _newsRepo.UpdateNewsArticle(NewsArticle);

            var currentTags = _tagRepo.GetTagsByArticleId(NewsArticle.NewsArticleId).ToList();

            var selectedTagIds = new List<int>();
            if (!string.IsNullOrEmpty(SelectedTags))
            {
                selectedTagIds = SelectedTags.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(int.Parse)
                    .ToList();
            }

            foreach (var tag in currentTags)
            {
                if (!selectedTagIds.Contains(tag.TagId))
                {
                    _newsRepo.RemoveTagFromNewsArticle(NewsArticle.NewsArticleId, tag.TagId);
                }
            }

            foreach (var tagId in selectedTagIds)
            {
                if (!currentTags.Any(t => t.TagId == tagId))
                {
                    _newsRepo.AddTagToNewsArticle(NewsArticle.NewsArticleId, tagId);
                }
            }

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
                        var newTag = new Tag
                        {
                            TagName = tagName
                        };

                        _tagRepo.AddTag(newTag);

                        existingTag = _tagRepo.GetTagByName(tagName);
                    }

                    if (!currentTags.Any(t => t.TagId == existingTag.TagId))
                    {
                        _newsRepo.AddTagToNewsArticle(NewsArticle.NewsArticleId, existingTag.TagId);
                    }
                }
            }

            return RedirectToPage("./Index");
        }
    }
}