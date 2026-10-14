using CareerBridge.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/transitionpaths")]
public class TransitionPathsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.TransitionPaths.AsNoTracking().ToListAsync());
}
