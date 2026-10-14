using CareerBridge.Shared;

namespace CareerBridge.Api.Data;

public static class PromptCatalog
{
    private static readonly Dictionary<TransitionStage, List<string>> Prompts = new()
    {
        [TransitionStage.Exploring] = new()
        {
            "What made you decide this new field was worth the jump, instead of a different one?",
            "What's one skill from my current job that you didn't expect to carry over, but did?",
            "Looking back, what would have made the first month easier for you?"
        },
        [TransitionStage.ResumeRewrite] = new()
        {
            "How did you describe your old job in industry language the new field would recognize?",
            "Which of your old accomplishments did hiring managers actually care about?",
            "What's one resume mistake you made early on that you'd tell me to skip?"
        },
        [TransitionStage.SkillBuilding] = new()
        {
            "Which skill gap took the longest to close, and how did you close it?",
            "Did you learn mostly through courses, projects, or on-the-job — and would you do it the same way again?",
            "What's a skill you spent time on that turned out not to matter much?"
        },
        [TransitionStage.Networking] = new()
        {
            "How did you find your first real contact in this field?",
            "What did you say when someone asked why you were switching careers?",
            "Is there a community, group, or event that mattered more than others?"
        },
        [TransitionStage.Interviewing] = new()
        {
            "What question about your background came up every time, and how did you answer it?",
            "How did you handle not having direct experience when asked for examples?",
            "What's something you wish you'd asked the interviewer, looking back?"
        },
        [TransitionStage.Negotiating] = new()
        {
            "Did switching fields cost you leverage on salary, and how did you handle that?",
            "What non-salary terms mattered more than you expected (training, ramp-up time, title)?",
            "How did you know an offer was fair for someone making this transition?"
        },
        [TransitionStage.Offer] = new()
        {
            "What surprised you most in your first 90 days in the new role?",
            "Is there anything you'd have negotiated differently before starting?",
            "What's the one thing from your old job you still find yourself relying on?"
        },
    };

    public static List<string> GetPrompts(TransitionStage stage) =>
        Prompts.TryGetValue(stage, out var list) ? list : new List<string>();
}
