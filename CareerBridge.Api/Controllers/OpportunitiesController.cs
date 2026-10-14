using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/opportunities")]
public class OpportunitiesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByPath([FromQuery] int? transitionPathId)
    {
        var query = db.Opportunities.AsNoTracking();
        var results = transitionPathId.HasValue
            ? await query.Where(o => o.TransitionPathId == transitionPathId.Value)
                .OrderByDescending(o => o.CreatedAt).ToListAsync()
            : await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOpportunityRequest req)
    {
        var opportunity = new Opportunity
        {
            PostedByUserId = req.PostedByUserId,
            TransitionPathId = req.TransitionPathId,
            Title = req.Title,
            Company = req.Company,
            Description = req.Description,
            Url = req.Url
        };
        db.Opportunities.Add(opportunity);
        await db.SaveChangesAsync();
        return Ok(opportunity);
    }
}
