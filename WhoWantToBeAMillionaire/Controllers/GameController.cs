using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        public List<QuestionModel> GetQuestions(int? roomId = null)
        {
            List<QuestionModel> questions = new List<QuestionModel>();
            if (roomId == null)
            {
                questions = _dataContext.Questions
                                        .ToList();
                return questions;
            }
            else
            {
                questions = _dataContext.Questions
                                        .Where(q => q.RoomId == roomId)
                                        .ToList();
            }

            var easyQuestions = questions
                                .Where(q => q.Difficulty == 1) // Mức độ dễ
                                .OrderBy(q => Guid.NewGuid()) // Random
                                .Take(5) // Lấy 5 câu
                                .ToList();

            var mediumQuestions = questions
                                  .Where(q => q.Difficulty == 2) // Mức độ trung bình
                                  .OrderBy(q => Guid.NewGuid())
                                  .Take(5)
                                  .ToList();

            var hardQuestions = questions
                                .Where(q => q.Difficulty == 3) // Mức độ khó
                                .OrderBy(q => Guid.NewGuid())
                                .Take(5)
                                .ToList();

            var allQuestions = easyQuestions
                               .Concat(mediumQuestions)
                               .Concat(hardQuestions)
                               .ToList();
            return allQuestions;
        }

        public IActionResult StartGame()
        {
            return View();
        }

        [HttpPost]
        public IActionResult StartGame(string play, string roomCode = "")
        {
            int roomId = 1;
            List<QuestionModel> questions = new List<QuestionModel>();
            if (play == "rand")
            {
                questions = GetQuestions();
            }
            else
            {
                var room = _dataContext.Rooms
                                       .FirstOrDefault(q => q.RoomCode == roomCode);
                if (room == null)
                {
                    TempData["notice"] = "*mã phòng không đúng!";
                    return View();
                }
                questions = GetQuestions(room.Id);
                roomId = room.Id;
            }
            // Lưu danh sách câu hỏi vào Session
            HttpContext.Session.Set("Questions", questions);
            HttpContext.Session.SetInt32("CurrentQuestionIndex", 0);
            HttpContext.Session.SetInt32("Score", 0);
            HttpContext.Session.SetInt32("RoomId", roomId);
            // Lưu thời gian bắt đầu
            HttpContext.Session.Set("StartTime", DateTime.Now);

            return RedirectToAction("Question");
        }

        public async Task<IActionResult> EndGame()
        {
            // Lấy dữ liệu từ Session
            var score = HttpContext.Session.GetInt32("Score") ?? 0;
            var roomId = HttpContext.Session.GetInt32("RoomId") ?? 0;
            var startTime = HttpContext.Session.Get<DateTime>("StartTime");
            var endTime = DateTime.Now;

            // Tính toán thời gian hoàn thành
            var duration = endTime - startTime;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId != null)
            {
                var gameHistory = new HistoryModel
                {
                    UserId = userId,
                    RoomId = roomId,
                    PlayedAt = startTime,
                    Score = score,
                    Completed = true,
                    Duration = duration
                };

                _dataContext.Histories.Add(gameHistory);
                await _dataContext.SaveChangesAsync();
            }

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
                    return Json("End");
                }
                else
                { 
                    currentIndex++;
                    HttpContext.Session.SetInt32("CurrentQuestionIndex", currentIndex);
                }
            }
            else return Json("Wrong");

            return Json("Correct"); // Hiển thị câu hỏi tiếp theo
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

        public IActionResult CallPhoneFriend()
        {
            var questions = HttpContext.Session.Get<List<QuestionModel>>("Questions");
            var currentIndex = HttpContext.Session.GetInt32("CurrentQuestionIndex") ?? 0;
            var currentQuestion = questions[currentIndex];
            currentQuestion.ShuffleAnswers(); // Trộn câu trả lời


            // Người thân sẽ đưa ra đáp án với xác suất ngẫu nhiên đúng/sai
            Random rand = new Random();
            int chance = rand.Next(100); // Xác suất từ 0 đến 99

            // 70% đưa ra đáp án đúng, 30% đưa ra đáp án sai
            string selectedAnswer;
            if (chance < 70)
            {
                selectedAnswer = currentQuestion.CorrectAnswer; // Đáp án đúng
            }
            else
            {
                var wrongAnswers = currentQuestion.ShuffledAnswers
                                                  .Where(a => a != currentQuestion.CorrectAnswer)
                                                  .OrderBy(x => Guid.NewGuid())
                                                  .First();
                selectedAnswer = wrongAnswers; // Đáp án sai
            }

            // Tạo thông báo phản hồi từ người thân
            var messages = new List<string>
            {
                $"Mình nghĩ đáp án đúng là {selectedAnswer}.",
                $"Theo mình thì đáp án là {selectedAnswer}.",
                $"Mình không chắc lắm, nhưng mình chọn {selectedAnswer}.",
                $"Có thể là {selectedAnswer}, nhưng bạn nên cân nhắc."
            };

            string message = messages[rand.Next(messages.Count)];

            return Json(new { message = message });
        }

        public IActionResult AskAdvisors()
        {
            // Dữ liệu giả lập
            var advisors = new List<object>
            {
                new { Name = "Chuyên gia Nam", Answer = "A", Comment = "Tôi nghĩ đáp án đúng là A." },
                new { Name = "Giáo sư Lan", Answer = "B", Comment = "Tôi không chắc lắm, nhưng có thể là B." },
                new { Name = "Bạn Minh", Answer = "C", Comment = "Theo tôi thì đáp án là C." }
            };

            return Json(advisors); // Trả về danh sách ý kiến của tổ tư vấn
        }

    }
}
