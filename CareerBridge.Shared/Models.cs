namespace CareerBridge.Shared;

public enum TransitionStage
{
    Exploring,
    ResumeRewrite,
    SkillBuilding,
    Networking,
    Interviewing,
    Negotiating,
    Offer
}

public class TransitionPath
{
    public int Id { get; set; }
    public string FromRole { get; set; } = "";
    public string ToRole { get; set; } = "";
    public string Industry { get; set; } = "";
}

public class UserProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Skills { get; set; } = "";
    public bool IsMentor { get; set; }
    public int TransitionPathId { get; set; }
    public TransitionPath? TransitionPath { get; set; }
    public DateTime? TransitionCompletedDate { get; set; }
    public TransitionStage CurrentStage { get; set; } = TransitionStage.Exploring;
}

public class PlaybookEntry
{
    public int Id { get; set; }
    public int AuthorUserId { get; set; }
    public UserProfile? Author { get; set; }
    public int TransitionPathId { get; set; }
    public TransitionPath? TransitionPath { get; set; }
    public string BudgetImpact { get; set; } = "";
    public int TimeTakenMonths { get; set; }
    public string HardestSkillGap { get; set; } = "";
    public string BiggestMistake { get; set; } = "";
    public string AdviceText { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MentorMatch
{
    public int MentorId { get; set; }
    public string MentorName { get; set; } = "";
    public string MentorBio { get; set; } = "";
    public string MentorSkills { get; set; } = "";
    public DateTime? TransitionCompletedDate { get; set; }
    public int MonthsSinceTransition { get; set; }
    public bool IsRevealed { get; set; }
    public int? ConnectionId { get; set; }
    public ConnectionStatus? ConnectionStatus { get; set; }
    public bool HasVerifiedBadge { get; set; }
}

public class CreateUserRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Skills { get; set; } = "";
    public bool IsMentor { get; set; }
    public int TransitionPathId { get; set; }
    public DateTime? TransitionCompletedDate { get; set; }
}

public class CreatePlaybookRequest
{
    public int AuthorUserId { get; set; }
    public int TransitionPathId { get; set; }
    public string BudgetImpact { get; set; } = "";
    public int TimeTakenMonths { get; set; }
    public string HardestSkillGap { get; set; } = "";
    public string BiggestMistake { get; set; } = "";
    public string AdviceText { get; set; } = "";
}

// ---- Micro-cohorts (#2) ----

public class Cohort
{
    public int Id { get; set; }
    public int TransitionPathId { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CohortMembership
{
    public int Id { get; set; }
    public int CohortId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class CohortView
{
    public int CohortId { get; set; }
    public string CohortName { get; set; } = "";
    public List<UserProfile> Members { get; set; } = new();
}

// ---- Skill-adjacency matching (#3) ----

public class SkillAdjacentMatch
{
    public int MentorId { get; set; }
    public string MentorName { get; set; } = "";
    public string MentorBio { get; set; } = "";
    public string MentorSkills { get; set; } = "";
    public string FromRole { get; set; } = "";
    public string ToRole { get; set; } = "";
    public int SharedSkillCount { get; set; }
    public List<string> SharedSkills { get; set; } = new();
}

// ---- Anonymous-first connections (#7 timeline + #12 anonymous reveal) ----

public enum ConnectionStatus { Requested, Accepted }

public class Connection
{
    public int Id { get; set; }
    public int MentorId { get; set; }
    public int MenteeId { get; set; }
    public ConnectionStatus Status { get; set; } = ConnectionStatus.Requested;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateConnectionRequest
{
    public int MentorId { get; set; }
    public int MenteeId { get; set; }
}

public class TimelineItem
{
    public int Id { get; set; }
    public int ConnectionId { get; set; }
    public string Title { get; set; } = "";
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateTimelineItemRequest
{
    public int ConnectionId { get; set; }
    public string Title { get; set; } = "";
}

public class ConnectionDetail
{
    public int Id { get; set; }
    public ConnectionStatus Status { get; set; }
    public UserProfile? Mentor { get; set; }
    public UserProfile? Mentee { get; set; }
    public List<TimelineItem> Timeline { get; set; } = new();
}

// ---- Financial runway check-ins (#8) ----

public class RunwayCheckIn
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MonthsOfRunwayRemaining { get; set; }
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateRunwayCheckInRequest
{
    public int UserId { get; set; }
    public int MonthsOfRunwayRemaining { get; set; }
    public string Notes { get; set; } = "";
}

public class RunwayCheckInView
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public int MonthsOfRunwayRemaining { get; set; }
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

// ---- Warm intro / opportunity referrals (#9) ----

public class Opportunity
{
    public int Id { get; set; }
    public int PostedByUserId { get; set; }
    public int TransitionPathId { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateOpportunityRequest
{
    public int PostedByUserId { get; set; }
    public int TransitionPathId { get; set; }
    public string Title { get; set; } = "";
    public string Company { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
}

// ---- Pay-it-forward mentor pipeline (#10) handled via UserProfile.IsMentor flip, no extra model ----

// ---- Verified-transition badges (#11) ----

public class Endorsement
{
    public int Id { get; set; }
    public int EndorserUserId { get; set; }
    public int EndorsedUserId { get; set; }
    public string Note { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateEndorsementRequest
{
    public int EndorserUserId { get; set; }
    public int EndorsedUserId { get; set; }
    public string Note { get; set; } = "";
}

// ---- Async "ask a cohort" board (#6) ----

public class Question
{
    public int Id { get; set; }
    public int TransitionPathId { get; set; }
    public int AuthorUserId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateQuestionRequest
{
    public int TransitionPathId { get; set; }
    public int AuthorUserId { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int AuthorUserId { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateAnswerRequest
{
    public int QuestionId { get; set; }
    public int AuthorUserId { get; set; }
    public string Body { get; set; } = "";
}

public class QuestionThread
{
    public Question Question { get; set; } = new();
    public string AuthorName { get; set; } = "";
    public List<AnswerView> Answers { get; set; } = new();
}

public class AnswerView
{
    public Answer Answer { get; set; } = new();
    public string AuthorName { get; set; } = "";
}
