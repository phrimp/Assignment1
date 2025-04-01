using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models;
using PhienNTMVC.Pages.Hubs;
using Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhienNTMVC.Pages.admin
{
    public class EditModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<UserHub> _hubContext;

        public EditModel(
            IAccountRepo accountRepo,
            IConfiguration configuration,
            IHubContext<UserHub> hubContext)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = default!;

        [BindProperty]
        public string? TempPassword { get; set; }

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

            // Don't show the actual password in the form
            TempPassword = "";

            // Configure role dropdown
            SetupRoleSelectList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

            // Get existing account information
            var existingAccount = _accountRepo.GetAccountById(SystemAccount.AccountId);
            if (existingAccount == null)
            {
                return NotFound();
            }

            // Remove password from model validation if empty
            if (string.IsNullOrEmpty(TempPassword))
            {
                // Keep the existing password
                SystemAccount.AccountPassword = existingAccount.AccountPassword;
                ModelState.Remove("SystemAccount.AccountPassword");
            }
            else
            {
                // Update with new password
                SystemAccount.AccountPassword = TempPassword;
            }

            // Validate the model excluding the password field
            if (!ModelState.IsValid)
            {
                SetupRoleSelectList();
                return Page();
            }

            // Check if email is changed and already exists
            if (existingAccount.AccountEmail != SystemAccount.AccountEmail &&
                _accountRepo.IsEmailExists(SystemAccount.AccountEmail, SystemAccount.AccountId))
            {
                ModelState.AddModelError("SystemAccount.AccountEmail", "This email is already in use by another account.");
                SetupRoleSelectList();
                return Page();
            }

            try
            {
                // Store the user name for SignalR notification
                string userName = SystemAccount.AccountName;
                int userId = SystemAccount.AccountId;

                _accountRepo.UpdateAccount(SystemAccount);

                // Send real-time notification via SignalR
                await _hubContext.Clients.All.SendAsync("UserUpdated", userId, userName);

                TempData["SuccessMessage"] = "User updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
                SetupRoleSelectList();
                return Page();
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