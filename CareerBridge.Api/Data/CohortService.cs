using CareerBridge.Shared;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Data;

public static class CohortService
{
    private const int MaxCohortSize = 6;

    // Groups 4-6 people on the same transition path into a named cohort,
    // instead of 1:1 pairing, so accountability isn't resting on one mentor.
    public static async Task AssignToCohortAsync(AppDbContext db, int transitionPathId, int userId)
    {
        var openCohort = await db.Cohorts
            .Where(c => c.TransitionPathId == transitionPathId)
            .FirstOrDefaultAsync(c => db.CohortMemberships.Count(m => m.CohortId == c.Id) < MaxCohortSize);

        if (openCohort is null)
        {
            var cohortNumber = await db.Cohorts.CountAsync(c => c.TransitionPathId == transitionPathId) + 1;
            openCohort = new Cohort { TransitionPathId = transitionPathId, Name = $"Cohort {cohortNumber}" };
            db.Cohorts.Add(openCohort);
            await db.SaveChangesAsync();
        }

        db.CohortMemberships.Add(new CohortMembership { CohortId = openCohort.Id, UserId = userId });
        await db.SaveChangesAsync();
    }
}
