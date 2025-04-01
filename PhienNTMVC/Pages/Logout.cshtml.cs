using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhienNTMVC.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();

            return RedirectToPage("/login");
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }
    }
}