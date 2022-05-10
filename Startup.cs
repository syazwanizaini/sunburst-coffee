using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Sunburst.Data;
using Stripe;

namespace SunCoffee
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method called by runtime. Use method to add service to container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AppDbContext")));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Account/Login");
                options.AccessDeniedPath = new PathString("/Account/AccessDenied");
                options.LogoutPath = new PathString("/Index");
            });

            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
        }

        // Import IServiceProvider package into the method
        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            // Create variables to sotre the RoleManager and UserManager
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // A list of roles
            string[] roleNames = { "Admin", "Member" };

            // Loop through all the roles from 3
            foreach (var roleName in roleNames)
            {
                // Check if the role exists
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to database
                    // Add the roles to the database using RoleManager
                    var roleResult = await RoleManager.
                        CreateAsync(new IdentityRole(roleName));
                }
            }
            // Find a user with the username and assign it to Admin role
            var _user = await UserManager.FindByEmailAsync("admin@sun.coffee");
            if (_user != null)
            {
                await UserManager.AddToRoleAsync(_user, "Admin");
            }
        }

        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            StripeConfiguration.ApiKey =
            Configuration.GetSection("Stripe")["SecretKey"];

            // To Create Roles and Set "admin@sun.coffee" as Admin
            CreateRoles(serviceProvider).Wait();

            // Use Secure Sockets
            app.UseHttpsRedirection();
            app.UseRouting();

            // Allow Us to use CSS and JS Files
            app.UseStaticFiles();

            // Autentication for Login
            app.UseAuthentication();
            app.UseAuthorization();

            // RazorPages are Used
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}

