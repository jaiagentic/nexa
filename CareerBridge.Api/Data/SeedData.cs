using CareerBridge.Shared;

namespace CareerBridge.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext db)
    {
        if (db.TransitionPaths.Any()) return;

        var paths = new[]
        {
            new TransitionPath { FromRole = "Retail Associate", ToRole = "UX Designer", Industry = "Tech" },
            new TransitionPath { FromRole = "Truck Driver", ToRole = "Logistics Coordinator", Industry = "Logistics" },
            new TransitionPath { FromRole = "Food Service Worker", ToRole = "Data Analyst", Industry = "Tech" },
        };
        db.TransitionPaths.AddRange(paths);
        db.SaveChanges();

        var retailToUx = paths[0];
        var truckToLogistics = paths[1];
        var foodToData = paths[2];

        var users = new[]
        {
            new UserProfile
            {
                Name = "Maria Chen", Email = "maria@example.com",
                Bio = "Made the retail-to-UX jump 14 months ago. Happy to help with portfolio reviews.",
                Skills = "Figma, User Research, Wireframing, Customer Empathy",
                IsMentor = true, TransitionPathId = retailToUx.Id,
                TransitionCompletedDate = DateTime.UtcNow.AddMonths(-14)
            },
            new UserProfile
            {
                Name = "James Okoro", Email = "james@example.com",
                Bio = "Pivoted from retail to UX 3 years ago, now a senior designer.",
                Skills = "Figma, Design Systems, UX Writing",
                IsMentor = true, TransitionPathId = retailToUx.Id,
                TransitionCompletedDate = DateTime.UtcNow.AddMonths(-36)
            },
            new UserProfile
            {
                Name = "Emily Johnson", Email = "emily@example.com",
                Bio = "Trucking to logistics coordination, finished my transition 8 months ago.",
                Skills = "Route Optimization, TMS Software, Inventory Planning, Customer Service",
                IsMentor = true, TransitionPathId = truckToLogistics.Id,
                TransitionCompletedDate = DateTime.UtcNow.AddMonths(-8)
            },
            new UserProfile
            {
                Name = "Luis Ramirez", Email = "luis@example.com",
                Bio = "Line cook turned data analyst. Completed my pivot 5 months ago via a part-time bootcamp.",
                Skills = "SQL, Excel, Python basics, Dashboarding",
                IsMentor = true, TransitionPathId = foodToData.Id,
                TransitionCompletedDate = DateTime.UtcNow.AddMonths(-5)
            },
            new UserProfile
            {
                Name = "Dana Whitfield", Email = "dana@example.com",
                Bio = "Currently in retail, working toward a UX role. Building my first portfolio piece.",
                Skills = "Customer Service, Visual Merchandising",
                IsMentor = false, TransitionPathId = retailToUx.Id,
                CurrentStage = TransitionStage.SkillBuilding
            },
            new UserProfile
            {
                Name = "Tom Berglund", Email = "tom@example.com",
                Bio = "Long-haul driver exploring logistics coordination roles.",
                Skills = "CDL License, Logistics basics",
                IsMentor = false, TransitionPathId = truckToLogistics.Id,
                CurrentStage = TransitionStage.Exploring
            },
        };
        db.Users.AddRange(users);
        db.SaveChanges();

        var maria = users[0];
        var james = users[1];
        var priya = users[2];

        var playbooks = new[]
        {
            new PlaybookEntry
            {
                AuthorUserId = maria.Id, TransitionPathId = retailToUx.Id,
                BudgetImpact = "Spent ~$400 on a UX certificate, lived on savings for 2 months between jobs.",
                TimeTakenMonths = 9,
                HardestSkillGap = "Translating customer-floor instincts into structured user research methods.",
                BiggestMistake = "I waited too long to apply, thinking my portfolio needed to be 'perfect' first.",
                AdviceText = "Apply before you feel ready. Hiring managers value retail customer empathy more than you'd think — lean into it in interviews instead of hiding your background."
            },
            new PlaybookEntry
            {
                AuthorUserId = james.Id, TransitionPathId = retailToUx.Id,
                BudgetImpact = "Self-taught with free resources, no major cost, but took longer because of it.",
                TimeTakenMonths = 24,
                HardestSkillGap = "Design systems thinking — retail doesn't prepare you for component-based design.",
                BiggestMistake = "I networked too late. The first two interviews I got were both through referrals I made in month 18, not from cold applications.",
                AdviceText = "Start networking in month 1, not after you feel 'qualified.' Join local UX meetups even before you have a portfolio."
            },
            new PlaybookEntry
            {
                AuthorUserId = priya.Id, TransitionPathId = truckToLogistics.Id,
                BudgetImpact = "Used unemployment benefits for 3 months while completing a TMS certification ($250).",
                TimeTakenMonths = 8,
                HardestSkillGap = "Spreadsheet and TMS software fluency — I'd never used Excel formulas before.",
                BiggestMistake = "I undersold my route-planning experience as 'just driving' instead of framing it as logistics optimization.",
                AdviceText = "Your route knowledge IS logistics experience. Reframe every trucking task in operations language on your resume before you apply."
            },
        };
        db.Playbooks.AddRange(playbooks);
        db.SaveChanges();

        var dana = users[4];
        var tom = users[5];

        // Micro-cohorts (#2): auto-assign the seeded mentees into cohorts the
        // same way real signups would be grouped.
        await CohortService.AssignToCohortAsync(db, dana.TransitionPathId, dana.Id);
        await CohortService.AssignToCohortAsync(db, tom.TransitionPathId, tom.Id);

        // Warm intro / opportunity referrals (#9).
        db.Opportunities.Add(new Opportunity
        {
            PostedByUserId = maria.Id,
            TransitionPathId = retailToUx.Id,
            Title = "Junior Product Designer",
            Company = "Northwind Software",
            Description = "My team is hiring a junior designer. They're open to non-traditional backgrounds — mention you came from Nexa.",
            Url = "https://example.com/jobs/junior-product-designer"
        });

        // Async "ask a cohort" board (#6).
        var question = new Question
        {
            TransitionPathId = retailToUx.Id,
            AuthorUserId = dana.Id,
            Title = "How do I talk about retail experience without it sounding irrelevant?",
            Body = "Every time I write a bullet point about my retail job it sounds like it has nothing to do with UX. How did you reframe it?"
        };
        db.Questions.Add(question);
        db.SaveChanges();

        db.Answers.Add(new Answer
        {
            QuestionId = question.Id,
            AuthorUserId = maria.Id,
            Body = "Reframe tasks as research and empathy work: 'Resolved 50+ customer issues daily' becomes 'Conducted informal user research identifying pain points across 50+ daily customer interactions.' Same work, design vocabulary."
        });

        // Verified-transition badges (#11): James has enough endorsements to show a badge.
        db.Endorsements.Add(new Endorsement { EndorserUserId = maria.Id, EndorsedUserId = james.Id, Note = "Mentored me well, legit career changer." });
        db.Endorsements.Add(new Endorsement { EndorserUserId = priya.Id, EndorsedUserId = james.Id, Note = "Verified his LinkedIn history matches his story." });

        db.SaveChanges();
    }
}
