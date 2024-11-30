using Microsoft.AspNetCore.Http;
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
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var score = HttpContext.Session.GetInt32("Score") ?? 0;

            var currentQuestion = questions[currentIndex];
            bool isCorrect = currentQuestion.CorrectAnswer == selectedAnswer;

            if (isCorrect)
            {
                score++;
                HttpContext.Session.SetInt32("Score", score);
            }

            // Chuyển sang câu hỏi tiếp theo
            HttpContext.Session.SetInt32("CurrentQuestionIndex", currentIndex + 1);

            if (currentIndex + 1 >= questions.Count)
            {
                return RedirectToAction("EndGame"); // Kết thúc game
            }

            return RedirectToAction("Question"); // Hiển thị câu hỏi tiếp theo
        }


        public IActionResult StartGame()
        {
            var questions = GetQuestionsByDifficulty();

            // Lưu danh sách câu hỏi vào Session
            HttpContext.Session.Set("Questions", questions);
            HttpContext.Session.SetInt32("CurrentQuestionIndex", 0);
            HttpContext.Session.SetInt32("Score", 0);

            return RedirectToAction("Question");
        }
        public List<QuestionModel> GetQuestionsByDifficulty()
        {
            var easyQuestions = _dataContext.Questions
                                        .Where(q => q.Difficulty == 1) // Mức độ dễ
                                        .OrderBy(q => Guid.NewGuid()) // Random
                                        .Take(5) // Lấy 5 câu
                                        .ToList();

            var mediumQuestions = _dataContext.Questions
                                          .Where(q => q.Difficulty == 2) // Mức độ trung bình
                                          .OrderBy(q => Guid.NewGuid())
                                          .Take(5)
                                          .ToList();

            var hardQuestions = _dataContext.Questions
                                        .Where(q => q.Difficulty == 3) // Mức độ khó
                                        .OrderBy(q => Guid.NewGuid())
                                        .Take(5)
                                        .ToList();

            // Kết hợp tất cả các câu hỏi thành một danh sách
            var allQuestions = easyQuestions
                               .Concat(mediumQuestions)
                               .Concat(hardQuestions)
                               .ToList();

            return allQuestions;
        }
        public IActionResult Question()
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;

            if (currentIndex >= questions.Count)
            {
                return RedirectToAction("EndGame");
            }

            var currentQuestion = questions[currentIndex];
            currentQuestion.ShuffleAnswers(); // Trộn câu trả lời

            return View(currentQuestion);
        }

        public IActionResult EndGame()
        {
            var score = HttpContext.Session.GetInt32("Score") ?? 0;
            ViewBag.Score = score;

            return View();
        }


    }

}
