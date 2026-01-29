using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this._unitOfWork = unitOfWork;
            this._webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var products = _unitOfWork.ProductRepository.GetAll(p => p.Include(p => p.Category)).ToList();

            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM vm = new ProductVM();
            if (id == null || id == 0)
            {
                vm.Product = new Product();
            }
            else
            {
                Product? productFromDb = _unitOfWork.ProductRepository.Get(u => u.Id == id, u => u.Include(u => u.ProductImages).Include(u => u.Category));
                if (productFromDb == null)
                {
                    return NotFound();
                }
                vm.Product = productFromDb;
            }
            vm.CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),

            });
            return View(vm);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {

                if (obj.Product.Id == 0 || obj.Product?.Id == null)
                {
                    _unitOfWork.ProductRepository.Add(obj.Product!);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(obj.Product);
                }
                _unitOfWork.Commit();

                var wwwrootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var productPath = @"images\products\product-" + fileName;
                        var finalPath = Path.Combine(wwwrootPath, productPath);
                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }
                        using (var filestream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        }
                        var productImage = new ProductImage()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = obj.Product.Id,
                        };

                        obj.Product.ProductImages ??= new List<ProductImage>();

                        obj.Product.ProductImages.Add(productImage);

                    }
                    _unitOfWork.ProductRepository.Update(obj.Product);
                    _unitOfWork.Commit();
                }
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");


            }
            obj.CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),

            });
            return View(obj);
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImageRepository.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(_webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImageRepository.Remove(imageToBeDeleted);
                _unitOfWork.Commit();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(p => p.Include(p => p.Category)).ToList();
            return Json(new { data = objProductList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.ProductRepository.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.ProductRepository.Remove(productToBeDeleted);
            _unitOfWork.Commit();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

    }
}
