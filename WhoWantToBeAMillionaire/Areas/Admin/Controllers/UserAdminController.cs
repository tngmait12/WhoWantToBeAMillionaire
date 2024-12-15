using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UserAdminController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UserAdminController(IWebHostEnvironment webHostEnvironment,DataContext dataContext, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;

        }
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id
                                        select new { User = u, RoleName = r.Name }).ToListAsync();
            return View(usersWithRoles);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existed_user = await _userManager.FindByIdAsync(id);
            if (existed_user == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (user.ImageUpload != null)
                {
                    //upload new image
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                    string imageName = Guid.NewGuid().ToString() + "_" + user.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    //delete old picture
                    if (existed_user.Image != null)
                    {
                        string oldfilePath = Path.Combine(uploadsDir, existed_user.Image);

                        try
                        {

                            if (System.IO.File.Exists(oldfilePath))
                            {
                                System.IO.File.Delete(oldfilePath);
                            }

                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("", "An error occurred while deleting the product image.");
                        }
                    }
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await user.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_user.Image = imageName;
                }
                existed_user.UserName = user.UserName;
                existed_user.Name = user.Name;
                existed_user.PhoneNumber = user.PhoneNumber;
                existed_user.RoleId = user.RoleId;
                //Cap nhat role

                // Lấy danh sách vai trò hiện tại của user
                var userRoles = await _dataContext.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync();

                // Xóa tất cả vai trò hiện tại của user
                if (userRoles.Any())
                {
                    _dataContext.UserRoles.RemoveRange(userRoles);
                }

                // Thêm vai trò mới
                var newRole = await _dataContext.Roles.FindAsync(existed_user.RoleId);
                if (newRole == null)
                {
                    return NotFound("Role not found.");
                }

                _dataContext.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = existed_user.Id,
                    RoleId = existed_user.RoleId
                });

                await _dataContext.SaveChangesAsync();

                var updateUserResult = await _userManager.UpdateAsync(existed_user);
                TempData["success"] = "Update User Success.";
                if (updateUserResult.Succeeded) return RedirectToAction("Index", "UserAdmin");
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existed_user);
                }
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            TempData["error"] = "Model đang có một vài thứ bị lỗi!!!";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);
            return View(existed_user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            await _userManager.DeleteAsync(user);
            TempData["success"] = "Xóa Người Dùng Thành Công.";
            return RedirectToAction("Index");
        }
        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
