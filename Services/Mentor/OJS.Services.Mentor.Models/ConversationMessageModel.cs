namespace OJS.Services.Mentor.Models;

using OJS.Common.Enumerations;

public class ConversationMessageModel
{
    public string Content { get; set; } = default!;

    public MentorMessageRole Role { get; set; }

    public int SequenceNumber { get; set; }

    public int ProblemId { get; set; }

    /// <summary>
    /// True if the problem description was extracted successfully from the resource document or the link.
    /// </summary>
    public bool ProblemIsExtractedSuccessfully { get; set; }
}