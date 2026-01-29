using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccesss.Repository.IRepository
{
    public interface IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; }
        public IProductRepository ProductRepository { get; }
        public IProductImageRepository ProductImageRepository { get; }
        void Commit();
    }
}
