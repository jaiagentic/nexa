using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/runway")]
public class RunwayController(AppDbContext db) : ControllerBase
{
    // One check-in per user, upserted — this is a running self-report, not a log.
    [HttpPost]
    public async Task<IActionResult> CheckIn(CreateRunwayCheckInRequest req)
    {
        var existing = await db.RunwayCheckIns.FirstOrDefaultAsync(r => r.UserId == req.UserId);
        if (existing is null)
        {
            existing = new RunwayCheckIn { UserId = req.UserId };
            db.RunwayCheckIns.Add(existing);
        }

        existing.MonthsOfRunwayRemaining = req.MonthsOfRunwayRemaining;
        existing.Notes = req.Notes;
        existing.CreatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(existing);
    }

    // Cohort-scoped so financial anxiety becomes a shared, normalized data point
    // instead of something each person carries alone (#8).
    [HttpGet("cohort/{cohortId:int}")]
    public async Task<IActionResult> GetForCohort(int cohortId)
    {
        var memberIds = await db.CohortMemberships.AsNoTracking()
            .Where(m => m.CohortId == cohortId).Select(m => m.UserId).ToListAsync();

        var checkIns = await db.RunwayCheckIns.AsNoTracking()
            .Where(r => memberIds.Contains(r.UserId)).ToListAsync();

        var users = await db.Users.AsNoTracking().Where(u => memberIds.Contains(u.Id)).ToListAsync();

        var views = checkIns.Select(r => new RunwayCheckInView
        {
            UserId = r.UserId,
            UserName = users.FirstOrDefault(u => u.Id == r.UserId)?.Name ?? "Unknown",
            MonthsOfRunwayRemaining = r.MonthsOfRunwayRemaining,
            Notes = r.Notes,
            CreatedAt = r.CreatedAt
        }).OrderBy(r => r.MonthsOfRunwayRemaining).ToList();

        return Ok(views);
    }
}
