using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models.ViewModels;
using WhoWantToBeAMillionaire.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Identity.UI.Services;
using WhoWantToBeAMillionaire.Areas.Admin.Data;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using WhoWantToBeAMillionaire.Controllers;

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
        private readonly IEmailSender _emailSender;
        public AccountAdminController(IEmailSender emailSender,IWebHostEnvironment webHostEnvironment,RoleManager<IdentityRole> roleManager, SignInManager<AppUserModel> signInManager, UserManager<AppUserModel> userManage, DataContext dataContext)
        {
            _userManage = userManage;
            _signInManager = signInManager;
            _dataContext = dataContext;
            _roleManager = roleManager;
            _emailSender = emailSender;
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
                    return RedirectToAction("login");
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

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email không tìm thấy.";
                return RedirectToAction("ForgotPassword", "AccountAdmin");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Đổi mật khẩu cho người dùng " + checkMail.Email;
                var message = "Click vào link để đổi mật khẩu " +
                    "" + $"{Request.Scheme}://{Request.Host}/Admin/AccountAdmin/NewPass?email=" + checkMail.Email + "&token=" + token + "";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }
            TempData["success"] = "Một email đã được gửi đến địa chỉ email đã đăng ký của bạn với hướng dẫn đặt lại mật khẩu.";
            return RedirectToAction("ForgotPassword", "AccountAdmin");
        }

        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkUser = await _userManage.Users.
                Where(u => u.Email == user.Email).Where(us => us.Token == token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                ViewBag.Email = user.Email;
                ViewBag.Token = user.Token;
            }
            else
            {
                TempData["success"] = "Email không tìm thấy hoặc token không đúng.";
                return RedirectToAction("ForgotPassword", "AccountAdmin");
            }

            return View();
        }

        public async Task<IActionResult> UpdateNewPass(AppUserModel user, string token)
        {
            var checkUser = await _userManage.Users.
                Where(u => u.Email == user.Email).Where(us => us.Token == token).FirstOrDefaultAsync();

            if (checkUser != null)
            {
                //update user with new password and new token
                string newToken = Guid.NewGuid().ToString();
                //hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkUser, user.PasswordHash);

                checkUser.PasswordHash = passwordHash;
                checkUser.Token = newToken;

                await _userManage.UpdateAsync(checkUser);
                TempData["success"] = "Cập nhật mật khẩu thành công.";
                return RedirectToAction("Login", "AccountAdmin");
            }
            else
            {
                TempData["success"] = "Không tìm thấy Email.";
                return RedirectToAction("ForgotPass", "AccountAdmin");
            }
        }

        //Đăng nhập, đăng ký bằng Google
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse"),

                    Items =
                                {
                                    { "prompt", "select_account" } // Thêm prompt=select_account để buộc hiển thị giao diện chọn tài khoản
								}
                });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            string emailName = email.Split('@')[0];

            var existing_user = await _userManage.FindByEmailAsync(email);
            if (existing_user == null)
            {
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedpassword = passwordHasher.HashPassword(null, "123456789");
                //Add Role
                var role = await _roleManager.FindByNameAsync("User");

                var newUser = new AppUserModel { UserName = emailName, Email = email, RoleId = role.Id };
                newUser.PasswordHash = hashedpassword;
                var createUserResult = await _userManage.CreateAsync(newUser);
                //Add user role
                var createUser = await _userManage.FindByEmailAsync(email);
                await _userManage.AddToRoleAsync(createUser, role.Name);


                if (!createUserResult.Succeeded)
                {
                    TempData["success"] = "Đăng ký tài khoản thất bại, Vui lòng thử lại.";
                    return RedirectToAction("Login", "AccountAdmin");
                }
                else
                {
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng ký tài khoản thành công.";
                    return Redirect("/Home/Index");
                }
            }
            else
            {
                await _signInManager.SignInAsync(existing_user, isPersistent: false);
            }
            TempData["success"] = "Đăng nhập thành công.";
            return Redirect("/Home/Index");
        }
    }
}
