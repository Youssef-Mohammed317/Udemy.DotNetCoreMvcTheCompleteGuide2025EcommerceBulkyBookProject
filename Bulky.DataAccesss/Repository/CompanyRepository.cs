using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;

namespace BulkyBook.DataAccesss.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
