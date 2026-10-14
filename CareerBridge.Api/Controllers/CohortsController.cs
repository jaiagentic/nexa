using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/cohorts")]
public class CohortsController(AppDbContext db) : ControllerBase
{
    [HttpGet("for-user/{userId:int}")]
    public async Task<IActionResult> GetForUser(int userId)
    {
        var membership = await db.CohortMemberships.AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId);
        if (membership is null) return NotFound();

        var cohort = await db.Cohorts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == membership.CohortId);
        if (cohort is null) return NotFound();

        var memberIds = await db.CohortMemberships.AsNoTracking()
            .Where(m => m.CohortId == cohort.Id).Select(m => m.UserId).ToListAsync();

        var members = await db.Users.AsNoTracking()
            .Where(u => memberIds.Contains(u.Id)).ToListAsync();

        return Ok(new CohortView { CohortId = cohort.Id, CohortName = cohort.Name, Members = members });
    }
}
