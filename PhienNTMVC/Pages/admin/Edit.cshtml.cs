using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Repository;

namespace PhienNTMVC.Pages.admin
{
    public class EditModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public EditModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = default!;

        public SelectList RoleSelectList { get; set; }

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

            // Configure role dropdown
            SetupRoleSelectList();

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
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

            if (!ModelState.IsValid)
            {
                SetupRoleSelectList();
                return Page();
            }

            // Check if email is changed and already exists
            var existingAccount = _accountRepo.GetAccountById(SystemAccount.AccountId);
            if (existingAccount != null &&
                existingAccount.AccountEmail != SystemAccount.AccountEmail &&
                _accountRepo.IsEmailExists(SystemAccount.AccountEmail))
            {
                ModelState.AddModelError("SystemAccount.AccountEmail", "This email is already in use by another account.");
                SetupRoleSelectList();
                return Page();
            }

            try
            {
                _accountRepo.UpdateAccount(SystemAccount);
                TempData["SuccessMessage"] = "User updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_accountRepo.GetAccountById(SystemAccount.AccountId).Equals(null))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private void SetupRoleSelectList()
        {
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Staff", Text = "Staff" },
                new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
            };

            // Map numeric role values to text
            if (SystemAccount.AccountRole == "0")
                SystemAccount.AccountRole = "Admin";
            else if (SystemAccount.AccountRole == "1")
                SystemAccount.AccountRole = "Staff";
            else if (SystemAccount.AccountRole == "2")
                SystemAccount.AccountRole = "Lecturer";

            RoleSelectList = new SelectList(roles, "Value", "Text", SystemAccount.AccountRole);
        }
    }
}