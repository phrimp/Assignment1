using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;
using Models;
using System.ComponentModel.DataAnnotations;

namespace PhienNTMVC.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public LoginModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new LoginInputModel();

        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }

        public void OnGet()
        {
            // Clear any previous login error message
            ErrorMessage = null;

            // If the user is already logged in, redirect to home page
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("email")))
            {
                Response.Redirect("/");
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var account = _accountRepo.Login(Input.Email, Input.Password);

            if (account != null)
            {
                // Store user information in session
                HttpContext.Session.SetString("email", account.AccountEmail);
                HttpContext.Session.SetString("name", account.AccountName);
                HttpContext.Session.SetString("role", account.AccountRole);
                HttpContext.Session.SetInt32("userId", account.AccountId);

                // Redirect based on role
                if (account.AccountRole == "Admin" ||
                    account.AccountRole == _configuration["AdminRole:RoleId"])
                {
                    return RedirectToPage("/admin/Index");
                }
                else if (account.AccountRole == "Staff" ||
                         account.AccountRole == _configuration["StaffRole:RoleId"])
                {
                    return RedirectToPage("/news/Index");
                }
                else if (account.AccountRole == "Lecturer" ||
                         account.AccountRole == _configuration["LecturerRole:RoleId"])
                {
                    return RedirectToPage("/news/Index");
                }

                // Default redirect
                return RedirectToPage("/Index");
            }

            // Login failed
            ErrorMessage = "Invalid email or password. Please try again.";
            return Page();
        }
    }
}