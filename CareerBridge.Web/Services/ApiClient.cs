using System.Net.Http.Json;
using System.Text.Json;
using CareerBridge.Shared;

namespace CareerBridge.Web.Services;

public class ApiClient(HttpClient http)
{
    // ASP.NET Core serializes with camelCase; HttpClient's JSON helpers default to
    // case-sensitive matching against our PascalCase models unless told otherwise.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<List<TransitionPath>> GetTransitionPathsAsync() =>
        await http.GetFromJsonAsync<List<TransitionPath>>("api/transitionpaths", Json) ?? new();

    public async Task<List<UserProfile>> GetUsersAsync() =>
        await http.GetFromJsonAsync<List<UserProfile>>("api/users", Json) ?? new();

    public async Task<UserProfile?> GetUserAsync(int id) =>
        await http.GetFromJsonAsync<UserProfile>($"api/users/{id}", Json);

    public async Task<UserProfile?> CreateUserAsync(CreateUserRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/users", req, Json);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<UserProfile>(Json);
    }

    public async Task UpdateStageAsync(int userId, TransitionStage stage)
    {
        var resp = await http.PutAsJsonAsync($"api/users/{userId}/stage", stage, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<List<MentorMatch>> GetMatchesForMenteeAsync(int menteeId) =>
        await http.GetFromJsonAsync<List<MentorMatch>>($"api/matches/for-mentee/{menteeId}", Json) ?? new();

    public async Task<List<string>> GetPromptsForStageAsync(TransitionStage stage) =>
        await http.GetFromJsonAsync<List<string>>($"api/prompts/stage/{stage}", Json) ?? new();

    public async Task<List<PlaybookEntry>> GetPlaybooksAsync(int? transitionPathId = null)
    {
        var url = transitionPathId.HasValue
            ? $"api/playbooks?transitionPathId={transitionPathId}"
            : "api/playbooks";
        return await http.GetFromJsonAsync<List<PlaybookEntry>>(url, Json) ?? new();
    }

    public async Task CreatePlaybookAsync(CreatePlaybookRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/playbooks", req, Json);
        resp.EnsureSuccessStatusCode();
    }

    // Micro-cohorts (#2)
    public async Task<CohortView?> GetCohortForUserAsync(int userId)
    {
        var resp = await http.GetAsync($"api/cohorts/for-user/{userId}");
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<CohortView>(Json);
    }

    // Skill-adjacency matching (#3)
    public async Task<List<SkillAdjacentMatch>> GetSkillAdjacentMatchesAsync(int menteeId) =>
        await http.GetFromJsonAsync<List<SkillAdjacentMatch>>($"api/matches/skill-adjacent/{menteeId}", Json) ?? new();

    // Connections: anonymous-first reveal + timelines (#7, #12)
    public async Task RequestIntroAsync(int mentorId, int menteeId)
    {
        var resp = await http.PostAsJsonAsync("api/connections/request",
            new CreateConnectionRequest { MentorId = mentorId, MenteeId = menteeId }, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<List<PendingIntro>> GetPendingIntrosForMentorAsync(int mentorId) =>
        await http.GetFromJsonAsync<List<PendingIntro>>($"api/connections/pending-for-mentor/{mentorId}", Json) ?? new();

    public async Task<List<PendingIntro>> GetAcceptedMenteesForMentorAsync(int mentorId) =>
        await http.GetFromJsonAsync<List<PendingIntro>>($"api/connections/accepted-for-mentor/{mentorId}", Json) ?? new();

    public async Task AcceptConnectionAsync(int connectionId)
    {
        var resp = await http.PutAsync($"api/connections/{connectionId}/accept", null);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<ConnectionDetail?> GetConnectionDetailAsync(int connectionId) =>
        await http.GetFromJsonAsync<ConnectionDetail>($"api/connections/{connectionId}", Json);

    public async Task AddTimelineItemAsync(int connectionId, string title)
    {
        var resp = await http.PostAsJsonAsync("api/timeline",
            new CreateTimelineItemRequest { ConnectionId = connectionId, Title = title }, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task ToggleTimelineItemAsync(int itemId)
    {
        var resp = await http.PutAsync($"api/timeline/{itemId}/toggle", null);
        resp.EnsureSuccessStatusCode();
    }

    // Financial runway check-ins (#8)
    public async Task SubmitRunwayCheckInAsync(CreateRunwayCheckInRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/runway", req, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<List<RunwayCheckInView>> GetCohortRunwayAsync(int cohortId) =>
        await http.GetFromJsonAsync<List<RunwayCheckInView>>($"api/runway/cohort/{cohortId}", Json) ?? new();

    // Warm intro / opportunity referrals (#9)
    public async Task<List<Opportunity>> GetOpportunitiesAsync(int? transitionPathId = null)
    {
        var url = transitionPathId.HasValue
            ? $"api/opportunities?transitionPathId={transitionPathId}"
            : "api/opportunities";
        return await http.GetFromJsonAsync<List<Opportunity>>(url, Json) ?? new();
    }

    public async Task CreateOpportunityAsync(CreateOpportunityRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/opportunities", req, Json);
        resp.EnsureSuccessStatusCode();
    }

    // Pay-it-forward (#10)
    public async Task<UserProfile?> GraduateAsync(int userId)
    {
        var resp = await http.PutAsync($"api/users/{userId}/graduate", null);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<UserProfile>(Json);
    }

    // Verified-transition badges (#11)
    public async Task EndorseAsync(int endorserUserId, int endorsedUserId, string note)
    {
        var resp = await http.PostAsJsonAsync("api/endorsements",
            new CreateEndorsementRequest { EndorserUserId = endorserUserId, EndorsedUserId = endorsedUserId, Note = note }, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task<List<Endorsement>> GetEndorsementsForUserAsync(int userId) =>
        await http.GetFromJsonAsync<List<Endorsement>>($"api/endorsements/for-user/{userId}", Json) ?? new();

    // Async "ask a cohort" board (#6)
    public async Task<List<QuestionThread>> GetQuestionsForPathAsync(int transitionPathId) =>
        await http.GetFromJsonAsync<List<QuestionThread>>($"api/questions?transitionPathId={transitionPathId}", Json) ?? new();

    public async Task AskQuestionAsync(CreateQuestionRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/questions", req, Json);
        resp.EnsureSuccessStatusCode();
    }

    public async Task AnswerQuestionAsync(CreateAnswerRequest req)
    {
        var resp = await http.PostAsJsonAsync("api/questions/answers", req, Json);
        resp.EnsureSuccessStatusCode();
    }
}

public class PendingIntro
{
    public Connection Connection { get; set; } = new();
    public UserProfile? Mentee { get; set; }
}
