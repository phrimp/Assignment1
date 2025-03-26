using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;

namespace PhienNTMVC.Pages.news
{
    public class EditModel : PageModel
    {
        private readonly Models.NewsSystemContext _context;

        public EditModel(Models.NewsSystemContext context)
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

            var newsarticle =  await _context.NewsArticles.FirstOrDefaultAsync(m => m.NewsArticleId == id);
            if (newsarticle == null)
            {
                return NotFound();
            }
            NewsArticle = newsarticle;
           ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
           ViewData["CreatedById"] = new SelectList(_context.SystemAccounts, "AccountId", "AccountEmail");
           ViewData["UpdatedById"] = new SelectList(_context.SystemAccounts, "AccountId", "AccountEmail");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(NewsArticle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsArticleExists(NewsArticle.NewsArticleId))
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

        private bool NewsArticleExists(int id)
        {
            return _context.NewsArticles.Any(e => e.NewsArticleId == id);
        }
    }
}
