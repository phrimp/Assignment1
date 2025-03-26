using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;

namespace PhienNTMVC.Pages
{
    public class loginModel : PageModel
    {
        IAccountRepo _accountRepo;
        public loginModel(IAccountRepo accountRepo)
        {
            _accountRepo = accountRepo;
        }
        public void OnGet()
        {
        }

        public void OnPost() {
            var email = Request.Form["email"];
            var password = Request.Form["password"];
            var ac = _accountRepo.Login(email, password);
            if (ac != null)
            {
                HttpContext.Session.SetString("email", email);
                Response.Redirect("/news");
            }
            
        }
    }
}
