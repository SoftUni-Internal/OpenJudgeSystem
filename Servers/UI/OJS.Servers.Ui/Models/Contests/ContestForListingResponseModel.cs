namespace OJS.Servers.Ui.Models.Contests;

using OJS.Services.Ui.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ContestForListingResponseModel : IMapFrom<ContestForListingServiceModel>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public bool CanBeCompeted { get; set; }

    public bool CanBePracticed { get; set; }

    public string Category { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int NumberOfProblems { get; set; }

    public int CompeteResults { get; set; }

    public int PracticeResults { get; set; }

    public int CompeteMaximumPoints { get; set; }

    public int PracticeMaximumPoints { get; set; }

    public ContestParticipantResultResponseModel? UserParticipationResult { get; set; }

    public bool RequirePasswordForCompete { get; set; }

    public bool RequirePasswordForPractice { get; set; }
}
