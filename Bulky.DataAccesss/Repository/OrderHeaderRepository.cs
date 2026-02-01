using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;

namespace BulkyBook.DataAccesss.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {

        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _dbSet.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = _dbSet.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderFromDb.PaymentIntentId = paymentIntentId;
                    orderFromDb.PaymentDate = DateTime.Now;
                }
            }
        }
    }
}
