namespace OJS.Services.Administration.Models.ExamGroups;

using OJS.Data.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;

public class ExamGroupInListModel : IMapFrom<ExamGroup>
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? ContestName { get; set; }

    public int? ContestId { get; set; }

    public string? ExternalAppId { get; set; }

    public int? ExternalExamGroupId { get; set; }
}