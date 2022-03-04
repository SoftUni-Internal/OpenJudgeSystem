namespace OJS.Servers.Administration.Models.Problems;

using OJS.Services.Administration.Models.Contests.Problems;
using SoftUni.AutoMapper.Infrastructure.Models;

public class ProblemRetestViewModel : IMapFrom<ProblemRetestServiceModel>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContestName { get; set; }

    public int ContestId { get; set; }

    public int SubmissionsCount { get; set; }
}