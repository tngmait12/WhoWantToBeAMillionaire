using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models.ViewModels;
using WhoWantToBeAMillionaire.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountAdminController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AccountAdminController(IWebHostEnvironment webHostEnvironment,RoleManager<IdentityRole> roleManager, SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManage, DataContext dataContext)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _dataContext = dataContext;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManage.GetUserAsync(User);

            var history = await _dataContext.Histories.Include(p => p.Room).Include(p => p.User).Where(p=>p.UserId==user.Id).ToListAsync();
            ViewBag.Historys = history;

            return View(user);
        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Đăng nhập thành công.";
                    return Redirect(loginVM.ReturnUrl ?? "/");

                }
                TempData["success"] = "Đăng nhập thất bại.";
                ModelState.AddModelError("", "Username hoặc Password không hợp lệ.!");
            }
            return View(loginVM);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel user)
        {

            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByNameAsync("User");

                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email, RoleId = role.Id };
                IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);

                var createUser = await _userManage.FindByEmailAsync(user.Email);


                await _userManage.AddToRoleAsync(createUser, role.Name);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Đăng ký tài khoản thành công!";
                    return Redirect("/accountadmin/login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string id)
        {
            var user = await _userManage.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string id,AppUserModel usermodel)
        {
            var user = await _userManage.FindByIdAsync(id);

            if (ModelState.IsValid)
            {
                
                if (usermodel.ImageUpload != null)
                {
                    //upload new image
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/users");
                    string imageName = Guid.NewGuid().ToString() + "_" + usermodel.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    //delete old picture
                    if(user.Image != null)
                    {
                        string oldfilePath = Path.Combine(uploadsDir, user.Image);

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
                    await usermodel.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    user.Image = imageName;
                }

                //hash the new pass
                user.PhoneNumber = usermodel.PhoneNumber;
                user.Name = usermodel.Name;

                await _userManage.UpdateAsync(user);
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model đang có một vài thứ bị lỗi!!!";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

    }
}
