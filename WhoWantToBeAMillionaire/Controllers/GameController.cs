using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
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

        public IActionResult EndGame()
        {
            var score = HttpContext.Session.GetInt32("Score") ?? 0;
            ViewBag.Score = score;

            return View();
        }

        public IActionResult DataQuestion()
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;

            //if (currentIndex >= questions.Count)
            //{

            //}

            var currentQuestion = questions[currentIndex];
            currentQuestion.ShuffleAnswers(); // Trộn câu trả lời

            if (currentQuestion != null)
            {
                var question = new
                {
                    index = currentIndex,
                    Content = currentQuestion.Content,
                    answer1 = currentQuestion.ShuffledAnswers[0],
                    answer2 = currentQuestion.ShuffledAnswers[1],
                    answer3 = currentQuestion.ShuffledAnswers[2],
                    answer4 = currentQuestion.ShuffledAnswers[3]
                };

                return Ok(question);
            }
            else
            {
                return BadRequest();
            }
        }

        public IActionResult CheckAnswer(string selectedAnswer)
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var score = HttpContext.Session.GetInt32("Score") ?? 0;

            var currentQuestion = questions[currentIndex];

            if (currentQuestion.CorrectAnswer == selectedAnswer)
            {
                score++;
                HttpContext.Session.SetInt32("Score", score);

                if (currentIndex + 1 >= questions.Count)
                {
                    return Json(false);
                }
                else
                { 
                    currentIndex++;
                    HttpContext.Session.SetInt32("CurrentQuestionIndex", currentIndex);
                }
            }
            else return Json(false);

            return Json(true); // Hiển thị câu hỏi tiếp theo
        }

        public IActionResult Question()
        {

            return View();
        }

        public IActionResult FiftyFifty()
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var currentQuestion = questions[currentIndex];
            currentQuestion.ShuffleAnswers(); // Trộn câu trả lời

            // Tìm hai đáp án sai
            var wrongAnswers = currentQuestion.ShuffledAnswers
                                              .Where(answer => answer != currentQuestion.CorrectAnswer)
                                              .OrderBy(x => Guid.NewGuid()) // Random hóa vị trí đáp án sai
                                              .Take(2)
                                              .ToList();

            return Json(wrongAnswers); // Trả về 2 đáp án sai đã chọn
        }

        public IActionResult AskAudience()
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var currentQuestion = questions[currentIndex];

            // Xác định câu trả lời đúng
            string correctAnswer = currentQuestion.CorrectAnswer;

            // Tạo phần trăm ngẫu nhiên cho các đáp án
            Random random = new Random();
            int correctPercent = random.Next(60, 90); // Xác suất cho đáp án đúng (60% - 90%)
            int remainingPercent = 100 - correctPercent;

            // Chia phần trăm còn lại cho các đáp án sai
            int wrongPercent1 = random.Next(0, remainingPercent);
            int wrongPercent2 = random.Next(0, remainingPercent - wrongPercent1);
            int wrongPercent3 = remainingPercent - wrongPercent1 - wrongPercent2;

            // Tạo danh sách kết quả
            var results = new List<object>
            {
                new { Answer = currentQuestion.CorrectAnswer, Percent = correctPercent},
                new { Answer = currentQuestion.Wrong_1, Percent = wrongPercent1 },
                new { Answer = currentQuestion.Wrong_2, Percent = wrongPercent2 },
                new { Answer = currentQuestion.Wrong_3, Percent = wrongPercent3}
            };

            // Trả về kết quả dưới dạng JSON
            return Json(results); // Ngẫu nhiên hóa thứ tự
        }


    }
}
