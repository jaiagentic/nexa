using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/timeline")]
public class TimelineController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add(CreateTimelineItemRequest req)
    {
        var item = new TimelineItem { ConnectionId = req.ConnectionId, Title = req.Title };
        db.TimelineItems.Add(item);
        await db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await db.TimelineItems.FindAsync(id);
        if (item is null) return NotFound();
        item.IsDone = !item.IsDone;
        await db.SaveChangesAsync();
        return Ok(item);
    }
}
