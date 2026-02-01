using BulkyBook.DataAccesss.Repository.IRepository;
using BulkyBook.Models.Entities;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public UserController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagment(string userId)
        {

            RoleManagmentVM RoleVM = new RoleManagmentVM()
            {
                ApplicationUser = _unitOfWork.UserRepository.Get(u => u.Id == userId, include: c => c.Include(c => c.Company)!),
                RoleList = _unitOfWork.UserRepository.GetAllRoles().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.CompanyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.ApplicationUser.Role = _unitOfWork.UserRepository.GetUserRole(_unitOfWork.UserRepository.Get(u => u.Id == userId));
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {

            ApplicationUser applicationUser = _unitOfWork.UserRepository.Get(u => u.Id == roleManagmentVM.ApplicationUser.Id);
            string oldRole = _unitOfWork.UserRepository.GetUserRole(applicationUser);



            if (!(roleManagmentVM.ApplicationUser.Role == oldRole)!)
            {
                //a role was updated
                if (roleManagmentVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.UserRepository.Update(applicationUser);
                _unitOfWork.Commit();

                _unitOfWork.UserRepository.ChangeRole(applicationUser, oldRole, roleManagmentVM.ApplicationUser.Role);

            }
            else
            {
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.UserRepository.Update(applicationUser);
                    _unitOfWork.Commit();
                }
            }

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _unitOfWork.UserRepository.GetAll(include: p => p.Include(p => p.Company)!).ToList();

            foreach (var user in users)
            {

                user.Role = _unitOfWork.UserRepository.GetUserRole(user);

                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = users });
        }

        [HttpPost]
        public IActionResult LockUnLock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.UserRepository.Get(u => u.Id == id, tracked: true);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _unitOfWork.UserRepository.Update(objFromDb);
            _unitOfWork.Commit();
            return Json(new { success = true, message = "Operation Successful" });
        }

        [HttpDelete]
        public IActionResult Delete(string? id)
        {
            var user = _unitOfWork.UserRepository.Get(u => u.Id == id);
            if (user != null)
            {
                _unitOfWork.UserRepository.Remove(user);
                _unitOfWork.Commit();
            }

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion

    }
}
