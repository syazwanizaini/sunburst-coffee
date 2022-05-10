using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
// To import MemoryStream
using System.IO;
using Sunburst.Data;

namespace Sunburst.Pages.Admin
{
    [Authorize(Roles ="Admin")]
    public class CreateModel : PageModel
    {
        private AppDbContext _db;
        [BindProperty]

        public Menu Menu { get; set; }

        public CreateModel(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) { return Page(); }
            Menu.Active = true;
            foreach (var file in Request.Form.Files)
            {
                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                Menu.ImageData = ms.ToArray();

                ms.Close();
                ms.Dispose();
            }

            _db.SunCoffee.Add(Menu);
            await _db.SaveChangesAsync();
            return RedirectToPage("/Admin/Create");
        }

        public void OnGet()
        {
        }

    }
}
