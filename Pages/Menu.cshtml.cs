using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Sunburst.Data;

namespace Sunburst.Pages
{
    public class MenuModel : PageModel
    {
        private readonly AppDbContext _db;
        public IList<Menu> SunCoffee { get; set; }
        [BindProperty]

        public string Search { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor
        public MenuModel(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
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
                "SELECT * FROM SunCoffee WHERE Active = 1 AND Name LIKE '%"
                + Search + "%'"
                ).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int itemID)
        {
            var item = await _db.SunCoffee.FindAsync(itemID);
            if(item != null)
            {
                _db.SunCoffee.Remove(item);
                await _db.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBuyAsync(int itemID)
        {
            var user = await _userManager.GetUserAsync(User);
            CheckoutCustomer customer = await _db
            .CheckoutCustomers
            .FindAsync(user.Email);

            var item =
            _db.BasketItems.FromSqlRaw("SELECT * FROM BasketItems WHERE StockID = {0}"
            + " AND BasketID = {1}", itemID, customer.BasketID)
            .ToList()
            .FirstOrDefault();

            if (item == null)
            {
                BasketItem newItem = new BasketItem
                {
                    BasketID = customer.BasketID,
                    StockID = itemID,
                    Quantity = 1
                };
                _db.BasketItems.Add(newItem);
                await _db.SaveChangesAsync();
            }
            else
            {
                item.Quantity = item.Quantity + 1;
                _db.Attach(item).State = EntityState.Modified;
                try
                {
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    throw new Exception($"Basket not found!", e);
                }
            }
            return RedirectToPage();
        }

    }
}
