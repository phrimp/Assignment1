using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;

namespace PhienNTMVC.Pages.admin
{
    public class DetailsModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public DetailsModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        public SystemAccount SystemAccount { get; set; } = default!;
        public IEnumerable<NewsArticle> CreatedArticles { get; set; } = new List<NewsArticle>();
        public IEnumerable<NewsArticle> UpdatedArticles { get; set; } = new List<NewsArticle>();

        public IActionResult OnGet(int? id)
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

            if (id == null)
            {
                return NotFound();
            }

            var account = _accountRepo.GetAccountById(id.Value);
            if (account == null)
            {
                return NotFound();
            }

            SystemAccount = account;

            // Get articles created by this user
            CreatedArticles = _accountRepo.GetArticlesCreatedByAccount(id.Value);

            // Get articles updated by this user
            UpdatedArticles = _accountRepo.GetArticlesUpdatedByAccount(id.Value);

            return Page();
        }
    }
}