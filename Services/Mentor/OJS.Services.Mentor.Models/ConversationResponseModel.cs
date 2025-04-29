namespace OJS.Services.Mentor.Models;

using AutoMapper;
using OJS.Services.Infrastructure.Models.Mapping;

public class ConversationResponseModel : IMapExplicitly
{
    public string UserId { get; set; } = default!;

    public int ProblemId { get; set; }

    public ICollection<ConversationMessageResponseModel> Messages { get; set; } = [];

    public int MaxUserInputLength { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<ConversationRequestModel, ConversationResponseModel>()
            .ForMember(dest => dest.MaxUserInputLength, opt => opt.Ignore());
}