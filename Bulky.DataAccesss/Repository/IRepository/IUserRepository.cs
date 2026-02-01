using BulkyBook.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace BulkyBook.DataAccesss.Repository.IRepository
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        void ChangeRole(ApplicationUser applicationUser, string oldRole, string newRole);
        List<IdentityRole> GetAllRoles();
        string GetUserRole(ApplicationUser user);
    }
}
