﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhoWantToBeAMillionaire.Data;
using WhoWantToBeAMillionaire.Models;

namespace WhoWantToBeAMillionaire.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class QuestionAdminController : Controller
    {
        private readonly DataContext _dataContext;
        public QuestionAdminController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Questions.OrderByDescending(p => p.Id).Include(p=>p.Topic).ToListAsync());
        }
        public IActionResult Create(int? roomId)
        {
            ViewBag.Topics = new SelectList(_dataContext.Topics, "Id", "Name");
            if (roomId.HasValue)
            {
                ViewBag.RoomId = roomId.Value;
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionModel question, int? roomId)
        {
            ViewBag.Topics = new SelectList(_dataContext.Topics, "Id", "Name", question.TopicId);
            if (ModelState.IsValid)
            {
                if (roomId.HasValue)
                {
                    question.RoomId = roomId.Value;
                }

                _dataContext.Add(question);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Add Question Success!";
                return RedirectToAction("Details", "RoomAdmin", new { id = roomId });
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

            return View(question);
        }

        public async Task<IActionResult> Edit(int Id, int? roomId)
        {
            QuestionModel question = await _dataContext.Questions.FirstOrDefaultAsync(p => p.Id == Id);
            ViewBag.Topics = new SelectList(_dataContext.Topics, "Id", "Name");
            if (roomId.HasValue)
            {
                ViewBag.RoomId = roomId.Value;
            }
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, QuestionModel question, int? roomId)
        {
            ViewBag.Topics = new SelectList(_dataContext.Topics, "Id", "Name", question.TopicId);

            var existed_question = _dataContext.Questions.Find(Id);
            if (ModelState.IsValid)
            {
                //Update
                existed_question.Content = question.Content;
                existed_question.CorrectAnswer = question.CorrectAnswer;
                existed_question.Wrong_1 = question.Wrong_1;
                existed_question.Wrong_3 = question.Wrong_3;
                existed_question.Wrong_2 = question.Wrong_2;
                existed_question.Difficulty = question.Difficulty;
                existed_question.TopicId = question.TopicId;
                //

                _dataContext.Update(existed_question);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Question Success!";
                return RedirectToAction("Details", "RoomAdmin", new { id = roomId });
                
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

            return View(question);
        }

        public async Task<IActionResult> Delete(int Id, int? roomId)
        {
            QuestionModel question = await _dataContext.Questions.FirstOrDefaultAsync(p => p.Id == Id);

            if (question == null)
            {
                return NotFound();
            }
            
            _dataContext.Questions.Remove(question);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Delete Question Success";
            return RedirectToAction("Details", "RoomAdmin", new { id = roomId });
        }
    }
}
