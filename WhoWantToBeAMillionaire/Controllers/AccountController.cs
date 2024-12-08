using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;
using WhoWantToBeAMillionaire.Models.ViewModels;

namespace WhoWantToBeAMillionaire.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage;
        private SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public AccountController(RoleManager<IdentityRole> roleManager, SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManage, DataContext dataContext)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _dataContext = dataContext;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
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
                    return Redirect("/account/login");
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
    }
}
