using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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

            var countRoom = _dataContext.Rooms.Count();
            var countUser = _dataContext.Users.Count();
            var CountTopic = _dataContext.Topics.Count();
            ViewBag.CountRoom = countRoom;
            ViewBag.CountTopic = CountTopic;
            ViewBag.CountUser = countUser;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetChartData()
        {
            var trafficData = await _dataContext.Statisticals
            .Where(s => s.DateAccess.Year == DateTime.UtcNow.Year)  // Lọc theo năm
            .GroupBy(s => s.DateAccess.Month)  // Nhóm theo tháng
            .Select(g => new
            {
                Month = g.Key,  // Tháng
                VisitorCount = g.Sum(s => s.CountAccess)  // Tính tổng số lượt truy cập
            })
            .OrderBy(d => d.Month)  // Sắp xếp theo tháng
            .ToListAsync();


            // Kiểm tra nếu không có dữ liệu
            if (!trafficData.Any())
            {
                return Json(new { message = "No data available", trafficData = Array.Empty<object>() });
            }

            return Json(trafficData);
        }
    }
}
