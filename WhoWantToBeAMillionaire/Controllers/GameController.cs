using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Controllers
{
    public class GameController : Controller
    {
        public readonly DataContext _dataContext;

        public GameController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public async Task<IActionResult> Index()
        {
            QuestionModel question = await _dataContext.Questions.Where(p=>p.Id==1).FirstOrDefaultAsync();
            question.ShuffleAnswers();
            return View(question);
        }

        [HttpPost]
        public IActionResult CheckAnswer(int id, string selectedAnswer)
        {
            var question = _dataContext.Questions.Find(id);

            if (question == null)
            {
                return NotFound("Không tìm thấy câu hỏi.");
            }

            bool isCorrect = question.CorrectAnswer == selectedAnswer;

            if (isCorrect)
            {
                TempData["success"] = "Correct!";
            }
            else
            {
                TempData["error"] = "Wrong!";
            }

            return RedirectToAction("Index");
        }
    }
}
