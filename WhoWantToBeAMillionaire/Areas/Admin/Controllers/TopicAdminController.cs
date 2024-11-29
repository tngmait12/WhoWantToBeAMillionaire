using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TopicAdminController : Controller
    {
        private readonly DataContext _dataContext;
        public TopicAdminController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Topics.OrderByDescending(p => p.Id).ToListAsync());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TopicModel topic)
        {
            if (ModelState.IsValid)
            {

                _dataContext.Add(topic);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Add Topic Success!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Model Error!!!";
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

            return View(topic);
        }

        public async Task<IActionResult> Edit(int Id)
        {
            TopicModel topic = await _dataContext.Topics.FirstOrDefaultAsync(p => p.Id == Id);
            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, TopicModel topic)
        {

            var existed_topic = _dataContext.Topics.Find(topic.Id);
            if (ModelState.IsValid)
            {


                //Update
                existed_topic.Name = topic.Name;
                existed_topic.Status = topic.Status;
                //

                _dataContext.Update(existed_topic);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Topic Success!";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model Error!!!";
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

            return View(topic);
        }

        public async Task<IActionResult> Delete(int Id)
        {
            TopicModel topic = await _dataContext.Topics.FirstOrDefaultAsync(p => p.Id == Id);

            if (topic == null)
            {
                return NotFound();
            }
            _dataContext.Topics.Remove(topic);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete Topic Success";

            return RedirectToAction("Index");
        }
    }
}
