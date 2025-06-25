namespace OJS.Services.Mentor.Models;

using OJS.Common.Enumerations;
using OJS.Services.Infrastructure.Models.Mapping;

public class ConversationMessageResponseModel : IMapFrom<ConversationMessageModel>
{
    public string Content { get; set; } = null!;

    public MentorMessageRole Role { get; set; }

    public int SequenceNumber { get; set; }
}