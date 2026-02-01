using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var companies = _unitOfWork.CompanyRepository.GetAll().ToList();

            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {
            Company vm;
            if (id == null || id == 0)
            {
                vm = new Company();
            }
            else
            {
                Company? companyFromDb = _unitOfWork.CompanyRepository.Get(u => u.Id == id);
                if (companyFromDb == null)
                {
                    return NotFound();
                }
                vm = companyFromDb;
            }

            return View(vm);
        }
        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {

                if (obj.Id == 0 || obj?.Id == null)
                {
                    _unitOfWork.CompanyRepository.Add(obj);
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(obj);
                }
                _unitOfWork.Commit();

                TempData["success"] = "Company created/updated successfully";
                return RedirectToAction("Index");

            }

            return View(obj);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.CompanyRepository.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.CompanyRepository.Remove(companyToBeDeleted);
            _unitOfWork.Commit();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

    }
}
