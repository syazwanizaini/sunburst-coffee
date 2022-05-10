using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sunburst.Data;
using System.IO;

namespace Sunburst.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditMenuModel : PageModel
    {
        private readonly AppDbContext _db;
        public EditMenuModel(AppDbContext db) { _db = db; }

        [BindProperty]
        public Menu Menu { get; set; }
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Menu = await _db.SunCoffee.FindAsync(id);
            if (Menu == null)
            {
                return RedirectToPage("/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Menu.Active = true;
            foreach (var file in Request.Form.Files)
            {
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                Menu.ImageData = ms.ToArray();

                ms.Close();
                ms.Dispose();
            }

            _db.Attach(Menu).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {
                throw new Exception($"Menu {Menu.ID} not found!", e);
            }
            return RedirectToPage("/Index");
        }

    }
}

