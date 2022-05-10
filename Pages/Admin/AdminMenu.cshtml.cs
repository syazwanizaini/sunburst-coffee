using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sunburst.Data;

namespace Sunburst.Admin
{
    [Authorize(Roles = "Admin")]
    public class MenuModel : PageModel
    {
        private readonly AppDbContext _db;
        public IList<Menu> SunCoffee { get; set; }
        [BindProperty]

        public string Search { get; set; }

        // Constructor
        public MenuModel(AppDbContext db)
        {
            _db = db;
        }
        public void OnGet()
        {
            SunCoffee = _db.SunCoffee.FromSqlRaw(
                "SELECT * FROM SunCoffee WHERE Active = 1"
                ).ToList();

        }

        public IActionResult OnPostSearch()
        {
            SunCoffee = _db.SunCoffee.FromSqlRaw(
                "SELECT * FROM SunCoffee WHERE Active = 1 AND Name LIKE '"
                + Search + "%'"
                ).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int menuID)
        {
            var menu = await _db.SunCoffee.FindAsync(menuID);
            menu.Active = false; // Set Item as Deleted
            _db.Attach(menu).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new Exception($"Menu {menu.ID} not found!", e);
            }
            return RedirectToPage();
        }


    }
}
