using BulkyBook.DataAccesss.Repository;
using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBook.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            this._emailSender = emailSender;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(filter: u => u.ApplicationUserId == userId,
                include: u => u.Include(u => u.Product)).ToList(),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = _unitOfWork.ProductImageRepository.GetAll(filter: u => u.ProductId == cart.ProductId).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(cart);
            _unitOfWork.Commit();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Remove(cart);
                _unitOfWork.Commit();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCartRepository.GetCartCount(cart.ApplicationUserId));
            }
            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cart);
                _unitOfWork.Commit();
            }


            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);

            _unitOfWork.ShoppingCartRepository.Remove(cart);

            _unitOfWork.Commit();


            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCartRepository.GetCartCount(cart.ApplicationUserId));


            return RedirectToAction(nameof(Index));

        }

        public IActionResult Summary()
        {
            var claimsIdenntiy = (ClaimsIdentity)User.Identity;
            var userId = claimsIdenntiy.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(
                    filter: u => u.ApplicationUserId == userId,
                    include: c => c.Include(c => c.Product)).ToList(),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.UserRepository.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber!;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress!;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City!;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State!;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode!;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdenntiy = (ClaimsIdentity)User.Identity;
            var userId = claimsIdenntiy.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(
                    filter: u => u.ApplicationUserId == userId,
                    include: c => c.Include(c => c.Product)).ToList();

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _unitOfWork.UserRepository.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // customer
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                // company
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }

            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Commit();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail detail = new OrderDetail()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetailRepository.Add(detail);
                _unitOfWork.Commit();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // need capture payment

                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = $"{domain}customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = $"{domain}customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",

                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }


                var service = new SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Commit();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }



            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });

        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(u => u.Id == id, include: c => c.Include(c => c.ApplicationUser));
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Commit();
                }
                HttpContext.Session.Clear();

            }

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
                $"<p>New Order Created - {orderHeader.Id}</p>").GetAwaiter();

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository
                .GetAll(filter: u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Commit();


            return View(id);
        }


        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if (cart.Count <= 50)
            {
                return cart.Product.Price;
            }
            else
            {
                if (cart.Count <= 100)
                {
                    return cart.Product.Price50;
                }
                else
                {
                    return cart.Product.Price100;
                }
            }
        }
    }
}
