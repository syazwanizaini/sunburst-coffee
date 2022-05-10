using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Sunburst.Data;

namespace Sunburst.Pages
{
    public class CheckoutModel : PageModel
    {
        // Create variables to hold the logged the database
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _UserManager;
        public IList<CheckoutItem> Items { get; private set; }
        public OrderHistory Order = new OrderHistory();

        public decimal Total = 0;
        public long AmountPayable = 0;

        // Instantiate the values for the database
        // The user manager through constructor dependency injection
        public CheckoutModel(AppDbContext db,
               UserManager<ApplicationUser> UserManager)
        {
            _db = db;
            _UserManager = UserManager;
        }

        // OnGet is called when the web page first loads, 
        // We use Async calls because it will access the database
        public async Task OnGetAsync()
        {
            // Use the UserManager to find out who the logged in user is
            var user = await _UserManager.GetUserAsync(User);
            CheckoutCustomer customer = await _db
                .CheckoutCustomers
                .FindAsync(user.Email);

            Items = _db.CheckoutItems.FromSqlRaw(
                "SELECT SunCoffee.ID, SunCoffee.Price, " +
                "SunCoffee.Name, " +
                "BasketItems.BasketID, BasketItems.Quantity " +
                "FROM SunCoffee INNER JOIN BasketItems " +
                "ON SunCoffee.ID = BasketItems.StockID " +
                "WHERE BasketID = {0}", customer.BasketID
                ).ToList();

            Total = 0;
            foreach (var item in Items)
            {
                Total += (item.Quantity * item.Price);
            }
            AmountPayable = (long)(Total * 100);
        }

        public async Task<IActionResult> OnPostAdditionalAsync (int itemID)
        {
            var user = await _UserManager.GetUserAsync(User);
            CheckoutCustomer customer = await _db
            .CheckoutCustomers
            .FindAsync(user.Email);

            var item =
            _db.BasketItems.FromSqlRaw("SELECT * FROM BasketItems " +
            "WHERE StockID = {0}"
            + " AND BasketID = {1}", itemID, customer.BasketID)
            .ToList()
            .FirstOrDefault();

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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSubtractAsync(int itemID)
        {
            var user = await _UserManager.GetUserAsync(User);
            CheckoutCustomer customer = await _db
            .CheckoutCustomers
            .FindAsync(user.Email);

            var item = _db.BasketItems.FromSqlRaw("SELECT * FROM BasketItems WHERE StockID = {0}" + " AND BasketID = {1}", itemID, customer.BasketID)
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
                item.Quantity = item.Quantity - 1;

                if (item.Quantity == 0)
                {
                    _db.BasketItems.Remove(item);
                }
                else
                {
                    _db.Attach(item).State = EntityState.Modified;
                }

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

        // Process
        public async Task Process()
        {
            var currentOrder = _db.OrderHistories
                .FromSqlRaw("SELECT * FROM OrderHistories")
                .OrderByDescending(b => b.OrderNo)
                .FirstOrDefault();

            if (currentOrder == null)
            {
                Order.OrderNo = 1;
            }
            else
            {
                Order.OrderNo = currentOrder.OrderNo + 1;
            }

            var user = await _UserManager.GetUserAsync(User);
            Order.Email = user.Email;
            _db.OrderHistories.Add(Order);

            CheckoutCustomer customer = await _db
                .CheckoutCustomers
                .FindAsync(user.Email);

            var basketItems =
                _db.BasketItems
                .FromSqlRaw("SELECT * From BasketItems " +
                "WHERE BasketID = {0}", customer.BasketID)
                .ToList();

            foreach (var item in basketItems)
            {
                Sunburst.Data.OrderItem oi = new Sunburst.Data.OrderItem
                {
                    OrderNo = Order.OrderNo,
                    StockID = item.StockID,
                    Quantity = item.Quantity
                };

                _db.OrderItems.Add(oi);
                _db.BasketItems.Remove(item);
            }
            await _db.SaveChangesAsync();
        }

        // Post Charge
        public IActionResult OnPostCharge(
            string stripeEmail,
            string stripeToken,
            long amount)
        {
            var customers = new CustomerService();
            var charges = new ChargeService();

            var customer = customers.Create(new CustomerCreateOptions
            {
                Email = stripeEmail,
                Source = stripeToken
            });

            var charge = charges.Create(new ChargeCreateOptions
            {
                Amount = amount,
                Description = "CO5227 SunCoffee charge",
                Currency = "gbp",
                Customer = customer.Id
            });

            Process().Wait();

            return RedirectToPage("/Account/Complete");
        }
    }

}
