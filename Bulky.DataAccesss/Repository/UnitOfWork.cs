using BulkyBook.DataAccesss.Data;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccesss.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }
        public ICategoryRepository CategoryRepository => _categoryRepository ?? new CategoryRepository(_context);
        public IProductRepository ProductRepository => _productRepository ?? new ProductRepository(_context);
        public IProductImageRepository ProductImageRepository => _productImageRepository ?? new ProductImageRepository(_context);
        public ICompanyRepository CompanyRepository => _companyRepository ?? new CompanyRepository(_context);
        public IShoppingCartRepository ShoppingCartRepository => _shoppingCartRepository ?? new ShoppingCartRepository(_context);
        public IOrderHeaderRepository OrderHeaderRepository => _orderHeaderRepository ?? new OrderHeaderRepository(_context);
        public IOrderDetailRepository OrderDetailRepository => _orderDetailRepository ?? new OrderDetailRepository(_context);

        public IUserRepository UserRepository => _userRepository ?? new UserRepository(_context, _userManager, _roleManager);

        public void Commit()
        {
            _context.SaveChanges();
        }
    }
}
