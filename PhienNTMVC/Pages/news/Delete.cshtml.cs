using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;

namespace PhienNTMVC.Pages.news
{
    public class DeleteModel : PageModel
    {
        private readonly Models.NewsSystemContext _context;

        public DeleteModel(Models.NewsSystemContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NewsArticle NewsArticle { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newsarticle = await _context.NewsArticles.FirstOrDefaultAsync(m => m.NewsArticleId == id);

            if (newsarticle == null)
            {
                return NotFound();
            }
            else
            {
                NewsArticle = newsarticle;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var newsarticle = await _context.NewsArticles.FindAsync(id);
            if (newsarticle != null)
            {
                NewsArticle = newsarticle;
                _context.NewsArticles.Remove(NewsArticle);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
