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

        // For displaying creator info even if CreatedBy is null
        public string CreatorName { get; set; } = "Unknown";
        public DateTime CreationDate { get; set; }

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

            try
            {
                // Get the news article with eager loading of related entities
                NewsArticle = _newsRepo.GetNewsArticleById(id.Value);

                if (NewsArticle == null)
                {
                    return NotFound();
                }

                // Store creation info separately to handle potential null CreatedBy
                CreationDate = NewsArticle.CreatedDate;
                if (NewsArticle.CreatedBy != null)
                {
                    CreatorName = NewsArticle.CreatedBy.AccountName;
                }
                else
                {
                    // If CreatedBy is null, try to get the creator's name from the account repository
                    var creator = _accountRepo.GetAccountById(NewsArticle.CreatedById);
                    if (creator != null)
                    {
                        CreatorName = creator.AccountName;

                        // Fix the missing CreatedBy reference
                        NewsArticle.CreatedBy = creator;
                    }
                }

                var currentUser = _accountRepo.GetAccountByEmail(userEmail);
                if (currentUser == null)
                {
                    return RedirectToPage("/login");
                }

                // Get active categories for dropdown
                CategorySelectList = new SelectList(
                    _categoryRepo.GetActiveCategories(),
                    "CategoryId",
                    "CategoryName");

                // Get all available tags
                AvailableTags = _tagRepo.GetAllTags().ToList();

                // Get current tags for this article
                CurrentTags = _tagRepo.GetTagsByArticleId(id.Value).ToList();

                return Page();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Edit OnGet: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the article for editing.";
                return RedirectToPage("./Index");
            }
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

            // Check validation
            

            try
            {
                // Get the original article to preserve creation info
                var originalArticle = _newsRepo.GetNewsArticleById(NewsArticle.NewsArticleId);
                if (originalArticle == null)
                {
                    TempData["ErrorMessage"] = "The article you're trying to edit no longer exists.";
                    return RedirectToPage("./Index");
                }

                // Preserve creation information
                NewsArticle.CreatedById = originalArticle.CreatedById;
                NewsArticle.CreatedDate = originalArticle.CreatedDate;

                // Set update metadata
                NewsArticle.UpdatedById = currentUser.AccountId;
                NewsArticle.ModifiedDate = DateTime.Now;

                // Update the article basic properties
                _newsRepo.UpdateNewsArticle(NewsArticle);
                Console.WriteLine("Article basic properties updated successfully");

                // Process tags
                ProcessTags(NewsArticle.NewsArticleId);

                TempData["SuccessMessage"] = "News article updated successfully.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging
                Console.WriteLine($"Error updating news article: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"Error updating news article: {ex.Message}";

                // Reload the page data
                LoadPageData(NewsArticle.NewsArticleId);
                return Page();
            }
        }
        private void LoadPageData(int articleId)
        {
            try
            {
                // Reload categories
                CategorySelectList = new SelectList(
                    _categoryRepo.GetActiveCategories(),
                    "CategoryId",
                    "CategoryName");

                // Reload tags
                AvailableTags = _tagRepo.GetAllTags().ToList();
                CurrentTags = _tagRepo.GetTagsByArticleId(articleId).ToList();

                // Get creation info
                var article = _newsRepo.GetNewsArticleById(articleId);
                if (article != null)
                {
                    CreationDate = article.CreatedDate;

                    if (article.CreatedBy != null)
                    {
                        CreatorName = article.CreatedBy.AccountName;
                    }
                    else
                    {
                        var creator = _accountRepo.GetAccountById(article.CreatedById);
                        CreatorName = creator?.AccountName ?? "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading page data: {ex.Message}");
                // Don't throw - we're already in an error handler
            }
        }

        // Helper method to process tags
        private void ProcessTags(int articleId)
        {
            try
            {
                // Get current tags
                var currentTags = _tagRepo.GetTagsByArticleId(articleId).ToList();
                Console.WriteLine($"Current tags count: {currentTags.Count}");

                // Process selected existing tags
                var selectedTagIds = new List<int>();
                if (!string.IsNullOrEmpty(SelectedTags))
                {
                    selectedTagIds = SelectedTags.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(int.Parse)
                        .ToList();
                    Console.WriteLine($"Selected tags count: {selectedTagIds.Count}");
                }

                // Remove tags that are no longer selected
                foreach (var tag in currentTags)
                {
                    if (!selectedTagIds.Contains(tag.TagId))
                    {
                        Console.WriteLine($"Removing tag {tag.TagId} - {tag.TagName}");
                        _newsRepo.RemoveTagFromNewsArticle(articleId, tag.TagId);
                    }
                }

                // Add newly selected tags
                foreach (var tagId in selectedTagIds)
                {
                    if (!currentTags.Any(t => t.TagId == tagId))
                    {
                        Console.WriteLine($"Adding tag {tagId}");
                        _newsRepo.AddTagToNewsArticle(articleId, tagId);
                    }
                }

                // Process new tags
                if (!string.IsNullOrEmpty(NewTags))
                {
                    var tagNames = NewTags.Split(',')
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();
                    Console.WriteLine($"New tags count: {tagNames.Count}");

                    foreach (var tagName in tagNames)
                    {
                        Console.WriteLine($"Processing new tag: {tagName}");
                        // Check if tag already exists
                        var existingTag = _tagRepo.GetTagByName(tagName);

                        if (existingTag == null)
                        {
                            // Create new tag
                            var newTag = new Tag
                            {
                                TagName = tagName
                            };
                            _tagRepo.AddTag(newTag);
                            Console.WriteLine($"Created new tag: {tagName}");
                            existingTag = _tagRepo.GetTagByName(tagName);
                        }

                        // Add tag to article if not already associated
                        if (existingTag != null && !currentTags.Any(t => t.TagId == existingTag.TagId))
                        {
                            Console.WriteLine($"Adding new tag {existingTag.TagId} - {existingTag.TagName} to article");
                            _newsRepo.AddTagToNewsArticle(articleId, existingTag.TagId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing tags: {ex.Message}");
                throw; // Rethrow to let the caller handle it
            }
        }
    }
}