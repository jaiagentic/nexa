using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/playbooks")]
public class PlaybooksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByPath([FromQuery] int? transitionPathId)
    {
        var query = db.Playbooks.AsNoTracking().Include(p => p.Author).Include(p => p.TransitionPath);
        var results = transitionPathId.HasValue
            ? await query.Where(p => p.TransitionPathId == transitionPathId.Value)
                .OrderByDescending(p => p.CreatedAt).ToListAsync()
            : await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePlaybookRequest req)
    {
        var entry = new PlaybookEntry
        {
            AuthorUserId = req.AuthorUserId,
            TransitionPathId = req.TransitionPathId,
            BudgetImpact = req.BudgetImpact,
            TimeTakenMonths = req.TimeTakenMonths,
            HardestSkillGap = req.HardestSkillGap,
            BiggestMistake = req.BiggestMistake,
            AdviceText = req.AdviceText
        };
        db.Playbooks.Add(entry);
        await db.SaveChangesAsync();
        return Ok(entry);
    }
}
