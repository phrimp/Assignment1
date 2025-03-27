using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;

namespace PhienNTMVC.Pages.admin
{
    public class DeleteModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public DeleteModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = default!;

        public bool DeleteDisabled { get; set; }
        public string ErrorMessage { get; set; }

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

            // Don't allow deleting your own account
            if (account.AccountId == currentUser.AccountId)
            {
                ErrorMessage = "You cannot delete your own account.";
                DeleteDisabled = true;
            }

            // Check if account has created or updated articles
            if (_accountRepo.HasCreatedArticles(id.Value))
            {
                ErrorMessage = "This user has created news articles and cannot be deleted.";
                DeleteDisabled = true;
            }
            else if (_accountRepo.HasUpdatedArticles(id.Value))
            {
                ErrorMessage = "This user has updated news articles and cannot be deleted.";
                DeleteDisabled = true;
            }

            return Page();
        }

        public IActionResult OnPost()
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

            if (SystemAccount == null || SystemAccount.AccountId == 0)
            {
                return NotFound();
            }

            // Don't allow deleting your own account
            if (SystemAccount.AccountId == currentUser.AccountId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToPage("./Index");
            }

            // Check if account has created or updated articles
            if (_accountRepo.HasCreatedArticles(SystemAccount.AccountId))
            {
                TempData["ErrorMessage"] = "This user has created news articles and cannot be deleted.";
                return RedirectToPage("./Index");
            }
            else if (_accountRepo.HasUpdatedArticles(SystemAccount.AccountId))
            {
                TempData["ErrorMessage"] = "This user has updated news articles and cannot be deleted.";
                return RedirectToPage("./Index");
            }

            var account = _accountRepo.GetAccountById(SystemAccount.AccountId);
            if (account != null)
            {
                try
                {
                    _accountRepo.DeleteAccount(SystemAccount.AccountId);
                    TempData["SuccessMessage"] = $"User '{account.AccountName}' was successfully deleted.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
                }
            }

            return RedirectToPage("./Index");
        }
    }
}