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
            var rooms = await _dataContext.Rooms.Where(r => r.HostPlayerId == userId).ToListAsync();
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

            // Xóa phòng khỏi cơ sở dữ liệu
            _dataContext.Rooms.Remove(room);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete Room Success.";
            // Chuyển hướng về danh sách phòng
            return RedirectToAction(nameof(Index));
        }
        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
