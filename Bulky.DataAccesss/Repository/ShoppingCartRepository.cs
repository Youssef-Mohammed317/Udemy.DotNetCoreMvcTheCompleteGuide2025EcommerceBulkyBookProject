using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;

namespace BulkyBook.DataAccesss.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {

        }

        public int GetCartCount(string userId)
        {
            return _dbSet.Where(c => c.ApplicationUserId == userId).Count();
        }
    }
}
