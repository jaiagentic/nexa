using CareerBridge.Api.Data;
using CareerBridge.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController(AppDbContext db) : ControllerBase
{
    private const int VerifiedEndorsementThreshold = 2;

    // "Few steps ahead" matching (#1) combined with anonymous-first browsing (#12):
    // mentors on the mentee's exact transition path, ranked by recency so fresh
    // memory of the hard parts outranks distant veterans. Until the mentee has an
    // accepted Connection with a given mentor, that mentor's identity stays masked
    // so people can browse pivot stories before revealing who they are.
    [HttpGet("for-mentee/{menteeId:int}")]
    public async Task<IActionResult> GetMatchesForMentee(int menteeId)
    {
        var mentee = await db.Users.FindAsync(menteeId);
        if (mentee is null) return NotFound("Mentee not found");

        var now = DateTime.UtcNow;

        var mentors = await db.Users
            .AsNoTracking()
            .Where(u => u.IsMentor && u.TransitionPathId == mentee.TransitionPathId
                        && u.TransitionCompletedDate != null)
            .ToListAsync();

        var connections = await db.Connections.AsNoTracking()
            .Where(c => c.MenteeId == menteeId).ToListAsync();

        var endorsementCounts = await db.Endorsements.AsNoTracking()
            .Where(e => mentors.Select(m => m.Id).Contains(e.EndorsedUserId))
            .GroupBy(e => e.EndorsedUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        var ranked = mentors
            .Select(m =>
            {
                var monthsSince = (int)Math.Round((now - m.TransitionCompletedDate!.Value).TotalDays / 30);
                var connection = connections.FirstOrDefault(c => c.MentorId == m.Id);
                var revealed = connection is { Status: ConnectionStatus.Accepted };
                return new MentorMatch
                {
                    MentorId = m.Id,
                    MentorName = m.Name,
                    MentorBio = m.Bio,
                    MentorSkills = m.Skills,
                    TransitionCompletedDate = m.TransitionCompletedDate,
                    MonthsSinceTransition = monthsSince,
                    IsRevealed = revealed,
                    ConnectionId = connection?.Id,
                    ConnectionStatus = connection?.Status,
                    HasVerifiedBadge = (endorsementCounts.FirstOrDefault(e => e.UserId == m.Id)?.Count ?? 0)
                                       >= VerifiedEndorsementThreshold
                };
            })
            // Sweet spot: 3-24 months out ranks highest (fresh memory, proven success).
            // Further-out mentors still show up, just ranked lower.
            .OrderBy(m => m.MonthsSinceTransition is >= 3 and <= 24 ? 0 : 1)
            .ThenBy(m => m.MonthsSinceTransition)
            .ToList();

        for (var i = 0; i < ranked.Count; i++)
        {
            if (!ranked[i].IsRevealed)
            {
                ranked[i].MentorName = $"Mentor {(char)('A' + i % 26)}";
            }
        }

        return Ok(ranked);
    }

    // Skill-adjacency matching (#3): instead of restricting to the mentee's exact
    // transition path, surface mentors anywhere whose skill set overlaps with the
    // mentee's — useful when the "official" path has no mentors yet, or when a
    // nearby pivot turns out to share more of the real skill gap.
    [HttpGet("skill-adjacent/{menteeId:int}")]
    public async Task<IActionResult> GetSkillAdjacentMatches(int menteeId)
    {
        var mentee = await db.Users.FindAsync(menteeId);
        if (mentee is null) return NotFound("Mentee not found");

        var menteeSkills = SplitSkills(mentee.Skills);
        if (menteeSkills.Count == 0) return Ok(new List<SkillAdjacentMatch>());

        var mentors = await db.Users.AsNoTracking()
            .Include(u => u.TransitionPath)
            .Where(u => u.IsMentor && u.Id != menteeId)
            .ToListAsync();

        var ranked = mentors
            .Select(m =>
            {
                var mentorSkills = SplitSkills(m.Skills);
                var shared = menteeSkills.Intersect(mentorSkills, StringComparer.OrdinalIgnoreCase).ToList();
                return new SkillAdjacentMatch
                {
                    MentorId = m.Id,
                    MentorName = m.Name,
                    MentorBio = m.Bio,
                    MentorSkills = m.Skills,
                    FromRole = m.TransitionPath?.FromRole ?? "",
                    ToRole = m.TransitionPath?.ToRole ?? "",
                    SharedSkillCount = shared.Count,
                    SharedSkills = shared
                };
            })
            .Where(m => m.SharedSkillCount > 0)
            .OrderByDescending(m => m.SharedSkillCount)
            .ToList();

        return Ok(ranked);
    }

    private static List<string> SplitSkills(string skills) =>
        skills.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
}
