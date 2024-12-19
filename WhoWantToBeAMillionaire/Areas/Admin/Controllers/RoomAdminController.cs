using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RoomAdminController : Controller
    {
        private readonly DataContext _dataContext;
        public RoomAdminController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRooms = await _dataContext.Rooms.Where(p=>p.HostPlayerId== userId).ToListAsync();
            foreach (var room in userRooms)
            {
                var user = _dataContext.Users.FirstOrDefault(u => u.Id == room.HostPlayerId);
                room.HostPlayer = user; // Gán tạm để hiển thị
            }
            ViewBag.UserRooms = userRooms;
            var rooms = await _dataContext.Rooms.ToListAsync();
            foreach (var room in rooms)
            {
                var user = _dataContext.Users.FirstOrDefault(u => u.Id == room.HostPlayerId);
                room.HostPlayer = user; // Gán tạm để hiển thị
            }
            return View(rooms);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomModel room)
        {
            if (ModelState.IsValid)
            {
                // Lấy UserId của người tạo
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Gắn thêm thông tin cho phòng
                room.RoomCode = GenerateRoomCode();
                room.HostPlayerId = userId;
                room.CreatedAt = DateTime.Now;

                // Lưu phòng vào cơ sở dữ liệu
                _dataContext.Rooms.Add(room);
                await _dataContext.SaveChangesAsync();

                // Chuyển hướng đến trang hiển thị phòng
                return RedirectToAction(nameof(Details), new { id = room.Id });
            }
            return View(room);
        }

        public async Task<IActionResult> Details(int id)
        {
            var room = await _dataContext.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            var user = _dataContext.Users.FirstOrDefault(u => u.Id == room.HostPlayerId);
            if (user != null)
            {
                string hostName = user.Name; // Hoặc user.UserName
                ViewBag.HostName = hostName;
            }

            // Kiểm tra nếu người dùng hiện tại là chủ phòng hoặc admin
            var isHost = User.Identity.Name == room.HostPlayer.UserName; // Email hoặc UserName
            var isAdmin = User.IsInRole("Admin");

            ViewBag.IsHost = isHost;
            ViewBag.IsAdmin = isAdmin;


            var questions = await _dataContext.Questions.Include(p=>p.Topic).Where(p => p.RoomId == id).ToListAsync();
            ViewBag.Questions = questions;

            var history = await _dataContext.Histories.Include(p=>p.Room).Include(p=>p.User).Where(p => p.RoomId == id).ToListAsync();
            ViewBag.Historys = history;

            var leaderboard = await _dataContext.Histories
                .Where(p=>p.RoomId == id)
                .GroupBy(h => h.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.FirstOrDefault().User.Name,
                    MaxScore = g.Max(h => h.Score), // Lấy điểm cao nhất
                    BestDuration = g.Where(h => h.Score == g.Max(h2 => h2.Score)) // Lấy danh sách có điểm cao nhất
                                    .OrderBy(h => h.Duration) // Sắp xếp theo thời gian hoàn thành
                                    .FirstOrDefault().Duration // Lấy thời gian tốt nhất
                })
                .OrderByDescending(x => x.MaxScore) // Sắp xếp theo điểm số
                .ThenBy(x => x.BestDuration) // Nếu điểm bằng nhau, sắp xếp theo thời gian hoàn thành
                .Take(10) // Giới hạn Top 10
                .ToListAsync();

            ViewBag.Leaderboard = leaderboard;

            return View(room);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Tìm phòng để xóa
            var room = await _dataContext.Rooms.FirstOrDefaultAsync(r => r.Id == id && r.HostPlayerId == userId);

            if (room == null)
            {
                return NotFound();
            }

            var questions = _dataContext.Questions.Where(r => r.RoomId == id);
            _dataContext.Questions.RemoveRange(questions);

            // Xóa phòng khỏi cơ sở dữ liệu
            _dataContext.Rooms.Remove(room);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete Room Success.";
            // Chuyển hướng về danh sách phòng
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleRoomStatus(int id)
        {
            var room = await _dataContext.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Đổi trạng thái IsActive
            room.IsActive = !room.IsActive;

            // Lưu thay đổi
            _dataContext.Rooms.Update(room);
            await _dataContext.SaveChangesAsync();

            // Trả về danh sách phòng hoặc trang quản lý
            return RedirectToAction("Details", "RoomAdmin", new { id = id });
        }
        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
