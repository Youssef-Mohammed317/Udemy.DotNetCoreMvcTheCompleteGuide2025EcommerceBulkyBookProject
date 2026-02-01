using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBook.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {

            var products = _unitOfWork.ProductRepository.GetAll(include: p => p.Include(p => p.Category).Include(p => p.ProductImages));
            return View(products);
        }

        public IActionResult Details(int productId)
        {

            var cart = new ShoppingCart()
            {
                Product = _unitOfWork.ProductRepository.Get(p => p.Id == productId, include: p => p.Include(p => p.Category).Include(p => p.ProductImages)),
                Count = 1,
                ProductId = productId
            };

            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.ApplicationUserId == userId &&
                    u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
                _unitOfWork.Commit();
            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                _unitOfWork.Commit();
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCartRepository.GetCartCount(userId));
            }
            TempData["success"] = "Cart updated successfully";


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
