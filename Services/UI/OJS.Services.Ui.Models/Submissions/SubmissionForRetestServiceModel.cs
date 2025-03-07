namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure.Models.Mapping;

public class SubmissionForRetestServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int ContestId { get; set; }

    public int? ContestCategoryId { get; set; }

    public int TestRunsCount { get; set; }

    public bool Processed { get; set; }

    public bool IsCompiledSuccessfully { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<Submission, SubmissionForRetestServiceModel>()
            .ForMember(m => m.UserId, opt => opt.MapFrom(s => s.Participant.UserId))
            .ForMember(m => m.ContestId, opt => opt.MapFrom(s => s.Participant.ContestId))
            .ForMember(m => m.ContestCategoryId, opt => opt.MapFrom(s => s.Participant.Contest.CategoryId))
            .ForMember(m => m.TestRunsCount, opt => opt.MapFrom(s => s.TestRuns.Count));
}