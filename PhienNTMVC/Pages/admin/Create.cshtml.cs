using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Repository;

namespace PhienNTMVC.Pages.admin
{
    public class CreateModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public CreateModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        public SelectList RoleSelectList { get; set; }

        [BindProperty]
        public SystemAccount SystemAccount { get; set; } = new SystemAccount();

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string PasswordMismatch { get; set; }

        public IActionResult OnGet()
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

            // Set up role dropdown list
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Staff", Text = "Staff" },
                new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
            };
            RoleSelectList = new SelectList(roles, "Value", "Text");

            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public IActionResult OnPost(string confirmPassword)
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

            // Check passwords match
            if (SystemAccount.AccountPassword != confirmPassword)
            {
                PasswordMismatch = "Passwords do not match.";

                // Re-establish the role dropdown
                var roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Staff", Text = "Staff" },
                    new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
                };
                RoleSelectList = new SelectList(roles, "Value", "Text", SystemAccount.AccountRole);

                return Page();
            }

            // Check email uniqueness
            if (_accountRepo.IsEmailExists(SystemAccount.AccountEmail))
            {
                ModelState.AddModelError("SystemAccount.AccountEmail", "This email is already in use.");

                // Re-establish the role dropdown
                var roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Staff", Text = "Staff" },
                    new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
                };
                RoleSelectList = new SelectList(roles, "Value", "Text", SystemAccount.AccountRole);

                return Page();
            }

            // Map role values if necessary
            if (SystemAccount.AccountRole == "Admin")
                SystemAccount.AccountRole = _configuration["AdminRole:RoleId"] ?? SystemAccount.AccountRole;
            else if (SystemAccount.AccountRole == "Staff")
                SystemAccount.AccountRole = _configuration["StaffRole:RoleId"] ?? SystemAccount.AccountRole;
            else if (SystemAccount.AccountRole == "Lecturer")
                SystemAccount.AccountRole = _configuration["LecturerRole:RoleId"] ?? SystemAccount.AccountRole;

            if (!ModelState.IsValid)
            {
                // Re-establish the role dropdown
                var roles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Staff", Text = "Staff" },
                    new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
                };
                RoleSelectList = new SelectList(roles, "Value", "Text", SystemAccount.AccountRole);

                return Page();
            }

            try
            {
                _accountRepo.AddAccount(SystemAccount);
                TempData["SuccessMessage"] = "User created successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
            }

            return RedirectToPage("./Index");
        }
    }
}