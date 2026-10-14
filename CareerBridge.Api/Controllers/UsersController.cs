using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Users.AsNoTracking().Include(u => u.TransitionPath).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await db.Users.AsNoTracking().Include(u => u.TransitionPath)
            .FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest req)
    {
        var user = new UserProfile
        {
            Name = req.Name,
            Email = req.Email,
            Bio = req.Bio,
            Skills = req.Skills,
            IsMentor = req.IsMentor,
            TransitionPathId = req.TransitionPathId,
            TransitionCompletedDate = req.TransitionCompletedDate,
            CurrentStage = TransitionStage.Exploring
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (!user.IsMentor)
        {
            await CohortService.AssignToCohortAsync(db, user.TransitionPathId, user.Id);
        }

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}/stage")]
    public async Task<IActionResult> UpdateStage(int id, [FromBody] TransitionStage stage)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.CurrentStage = stage;
        await db.SaveChangesAsync();
        return Ok(user);
    }

    // Pay-it-forward pipeline (#10): once someone completes their own pivot,
    // flip them into a mentor so they become the "few steps ahead" match for
    // the next cohort behind them.
    [HttpPut("{id:int}/graduate")]
    public async Task<IActionResult> Graduate(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.IsMentor) return Ok(user);

        user.IsMentor = true;
        user.TransitionCompletedDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(user);
    }
}
