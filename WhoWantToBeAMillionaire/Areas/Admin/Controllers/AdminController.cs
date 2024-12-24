using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        public AdminController(DataContext dataContext, UserManager<AppUserModel> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;

        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existed_user = await _userManager.FindByIdAsync(userId);
            ViewBag.User = existed_user;
            return View();
        }
    }
}
