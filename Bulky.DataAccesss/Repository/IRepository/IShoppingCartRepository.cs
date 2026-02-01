using BulkyBook.Models.Entities;

namespace BulkyBook.DataAccesss.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        int GetCartCount(string userId);
    }
}
