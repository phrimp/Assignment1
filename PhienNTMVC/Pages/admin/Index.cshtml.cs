using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Repository;

namespace PhienNTMVC.Pages.admin
{
    public class IndexModel : PageModel
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IConfiguration _configuration;

        public IndexModel(IAccountRepo accountRepo, IConfiguration configuration)
        {
            _accountRepo = accountRepo;
            _configuration = configuration;
        }

        public IList<SystemAccount> SystemAccount { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string RoleFilter { get; set; }

        public SelectList RoleList { get; set; }

        public int CurrentUserId { get; set; }

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

            CurrentUserId = currentUser.AccountId;

            // Set up role list for dropdown
            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Staff", Text = "Staff" },
                new SelectListItem { Value = "Lecturer", Text = "Lecturer" }
            };
            RoleList = new SelectList(roles, "Value", "Text");

            // Get accounts based on search/filter criteria
            if (!string.IsNullOrEmpty(SearchTerm) && !string.IsNullOrEmpty(RoleFilter))
            {
                SystemAccount = _accountRepo.SearchAccounts(SearchTerm)
                    .Where(a => a.AccountRole == RoleFilter ||
                           (RoleFilter == "Admin" && a.AccountRole == "0") ||
                           (RoleFilter == "Staff" && a.AccountRole == "1") ||
                           (RoleFilter == "Lecturer" && a.AccountRole == "2"))
                    .ToList();
            }
            else if (!string.IsNullOrEmpty(SearchTerm))
            {
                SystemAccount = _accountRepo.SearchAccounts(SearchTerm).ToList();
            }
            else if (!string.IsNullOrEmpty(RoleFilter))
            {
                SystemAccount = _accountRepo.GetAllAccounts()
                    .Where(a => a.AccountRole == RoleFilter ||
                           (RoleFilter == "Admin" && a.AccountRole == "0") ||
                           (RoleFilter == "Staff" && a.AccountRole == "1") ||
                           (RoleFilter == "Lecturer" && a.AccountRole == "2"))
                    .ToList();
            }
            else
            {
                SystemAccount = _accountRepo.GetAllAccounts().ToList();
            }

            return Page();
        }
    }
}