using BulkyBook.DataAccesss.Data;
using BulkyBook.Models.Entities;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccesss.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager
            ;
        public DbInitializer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this._context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public void Initialize()
        {
            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                var user = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    Name = "Youssef Mohammad",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Beltan"
                };
                _userManager.CreateAsync(user, "Admin@123").GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();

            }

            _context.SaveChanges();
        }
    }
}
