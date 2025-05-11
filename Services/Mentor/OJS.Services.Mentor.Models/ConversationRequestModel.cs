namespace OJS.Services.Mentor.Models;

public class ConversationRequestModel
{
    public string UserId { get; set; } = string.Empty;

    public ICollection<ConversationMessageModel> Messages { get; set; } = [];

    public int ProblemId { get; set; }

    public string ProblemName { get; set; } = string.Empty;

    public int ContestId { get; set; }

    public string ContestName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string SubmissionTypeName { get; set; } = string.Empty;
}