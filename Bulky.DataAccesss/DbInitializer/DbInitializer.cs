using BulkyBook.DataAccesss.Data;
using BulkyBook.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccesss.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;

        public DbInitializer(ApplicationDbContext context)
        {
            this._context = context;
        }
        public void Initialize()
        {
            _context.Database.EnsureCreated();

            if (_context.Database.GetPendingMigrations().Any())
            {
                _context.Database.Migrate();
            }

            _context.SaveChanges();
        }
    }
}
