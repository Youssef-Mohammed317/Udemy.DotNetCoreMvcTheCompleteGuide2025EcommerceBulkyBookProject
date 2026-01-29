using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccesss.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryRepository _categoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly ProductImageRepository _productImageRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            this._context = context;
        }
        public ICategoryRepository CategoryRepository => _categoryRepository ?? new CategoryRepository(_context);
        public IProductRepository ProductRepository => _productRepository ?? new ProductRepository(_context);
        public IProductImageRepository ProductImageRepository => _productImageRepository ?? new ProductImageRepository(_context);



        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
