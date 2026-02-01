using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;

namespace BulkyBook.DataAccesss.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
