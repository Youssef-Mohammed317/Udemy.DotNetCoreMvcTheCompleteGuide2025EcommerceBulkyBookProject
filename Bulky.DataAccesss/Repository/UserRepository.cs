using Azure.Core;
using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccesss.Repository
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(context)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }
        public List<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }
        public string GetUserRole(ApplicationUser user)
        {

            return _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault()!;

        }
        public void ChangeRole(ApplicationUser applicationUser, string oldRole, string newRole)
        {
            _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(applicationUser, newRole).GetAwaiter().GetResult();
        }
    }
}
