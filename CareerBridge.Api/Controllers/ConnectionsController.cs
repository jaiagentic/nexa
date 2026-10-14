using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/connections")]
public class ConnectionsController(AppDbContext db) : ControllerBase
{
    private static readonly string[] DefaultTimelineItems =
    [
        "Share your resume/background for a first review",
        "Talk through your target role and timeline",
        "Set a cadence for check-ins (e.g. biweekly)"
    ];

    // Mentee requests an intro after browsing an anonymized match (#12).
    [HttpPost("request")]
    public async Task<IActionResult> RequestIntro(CreateConnectionRequest req)
    {
        var existing = await db.Connections.FirstOrDefaultAsync(c =>
            c.MentorId == req.MentorId && c.MenteeId == req.MenteeId);
        if (existing is not null) return Ok(existing);

        var connection = new Connection { MentorId = req.MentorId, MenteeId = req.MenteeId };
        db.Connections.Add(connection);
        await db.SaveChangesAsync();
        return Ok(connection);
    }

    [HttpGet("pending-for-mentor/{mentorId:int}")]
    public async Task<IActionResult> GetPendingForMentor(int mentorId)
    {
        var pending = await db.Connections.AsNoTracking()
            .Where(c => c.MentorId == mentorId && c.Status == ConnectionStatus.Requested)
            .ToListAsync();

        var menteeIds = pending.Select(c => c.MenteeId).ToList();
        var mentees = await db.Users.AsNoTracking().Where(u => menteeIds.Contains(u.Id)).ToListAsync();

        var result = pending.Select(c => new
        {
            connection = c,
            mentee = mentees.FirstOrDefault(u => u.Id == c.MenteeId)
        });

        return Ok(result);
    }

    [HttpGet("accepted-for-mentor/{mentorId:int}")]
    public async Task<IActionResult> GetAcceptedForMentor(int mentorId)
    {
        var accepted = await db.Connections.AsNoTracking()
            .Where(c => c.MentorId == mentorId && c.Status == ConnectionStatus.Accepted)
            .ToListAsync();

        var menteeIds = accepted.Select(c => c.MenteeId).ToList();
        var mentees = await db.Users.AsNoTracking().Where(u => menteeIds.Contains(u.Id)).ToListAsync();

        var result = accepted.Select(c => new
        {
            connection = c,
            mentee = mentees.FirstOrDefault(u => u.Id == c.MenteeId)
        });

        return Ok(result);
    }

    [HttpPut("{id:int}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var connection = await db.Connections.FindAsync(id);
        if (connection is null) return NotFound();

        connection.Status = ConnectionStatus.Accepted;
        await db.SaveChangesAsync();

        if (!await db.TimelineItems.AnyAsync(t => t.ConnectionId == id))
        {
            foreach (var title in DefaultTimelineItems)
            {
                db.TimelineItems.Add(new TimelineItem { ConnectionId = id, Title = title });
            }
            await db.SaveChangesAsync();
        }

        return Ok(connection);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var connection = await db.Connections.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (connection is null) return NotFound();

        var mentor = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == connection.MentorId);
        var mentee = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == connection.MenteeId);
        var timeline = await db.TimelineItems.AsNoTracking()
            .Where(t => t.ConnectionId == id).OrderBy(t => t.Id).ToListAsync();

        return Ok(new ConnectionDetail
        {
            Id = connection.Id,
            Status = connection.Status,
            Mentor = mentor,
            Mentee = mentee,
            Timeline = timeline
        });
    }
}
