using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController(AppDbContext db) : ControllerBase
{
    // Async "ask a cohort" board (#6), scoped to a transition path so value
    // accumulates even when nobody is online at the same time.
    [HttpGet]
    public async Task<IActionResult> GetByPath([FromQuery] int transitionPathId)
    {
        var questions = await db.Questions.AsNoTracking()
            .Where(q => q.TransitionPathId == transitionPathId)
            .OrderByDescending(q => q.CreatedAt).ToListAsync();

        var authorIds = questions.Select(q => q.AuthorUserId).Distinct().ToList();
        var questionIds = questions.Select(q => q.Id).ToList();

        var answers = await db.Answers.AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId)).OrderBy(a => a.CreatedAt).ToListAsync();
        authorIds.AddRange(answers.Select(a => a.AuthorUserId));

        var users = await db.Users.AsNoTracking()
            .Where(u => authorIds.Contains(u.Id)).ToListAsync();

        string NameOf(int id) => users.FirstOrDefault(u => u.Id == id)?.Name ?? "Unknown";

        var threads = questions.Select(q => new QuestionThread
        {
            Question = q,
            AuthorName = NameOf(q.AuthorUserId),
            Answers = answers.Where(a => a.QuestionId == q.Id)
                .Select(a => new AnswerView { Answer = a, AuthorName = NameOf(a.AuthorUserId) })
                .ToList()
        }).ToList();

        return Ok(threads);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionRequest req)
    {
        var question = new Question
        {
            TransitionPathId = req.TransitionPathId,
            AuthorUserId = req.AuthorUserId,
            Title = req.Title,
            Body = req.Body
        };
        db.Questions.Add(question);
        await db.SaveChangesAsync();
        return Ok(question);
    }

    [HttpPost("answers")]
    public async Task<IActionResult> AddAnswer(CreateAnswerRequest req)
    {
        var answer = new Answer
        {
            QuestionId = req.QuestionId,
            AuthorUserId = req.AuthorUserId,
            Body = req.Body
        };
        db.Answers.Add(answer);
        await db.SaveChangesAsync();
        return Ok(answer);
    }
}
