using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/endorsements")]
public class EndorsementsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateEndorsementRequest req)
    {
        if (req.EndorserUserId == req.EndorsedUserId)
            return BadRequest("Cannot endorse yourself.");

        var existing = await db.Endorsements.FirstOrDefaultAsync(e =>
            e.EndorserUserId == req.EndorserUserId && e.EndorsedUserId == req.EndorsedUserId);
        if (existing is not null) return Ok(existing);

        var endorsement = new Endorsement
        {
            EndorserUserId = req.EndorserUserId,
            EndorsedUserId = req.EndorsedUserId,
            Note = req.Note
        };
        db.Endorsements.Add(endorsement);
        await db.SaveChangesAsync();
        return Ok(endorsement);
    }

    [HttpGet("for-user/{userId:int}")]
    public async Task<IActionResult> GetForUser(int userId) =>
        Ok(await db.Endorsements.AsNoTracking()
            .Where(e => e.EndorsedUserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync());
}
