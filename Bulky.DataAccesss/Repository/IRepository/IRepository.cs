using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BulkyBook.DataAccesss.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T Get(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!, bool tracked = false);
        void Update(T entity);
        void Remove(T entity);
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null!, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null!);
        void Add(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
