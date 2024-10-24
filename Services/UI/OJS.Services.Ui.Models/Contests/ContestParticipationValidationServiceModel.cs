namespace OJS.Services.Ui.Models.Contests;

using OJS.Common.Enumerations;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ContestParticipationValidationServiceModel
    : IMapFrom<ContestRegistrationDetailsServiceModel>,
    IMapFrom<ContestServiceModel>,
    IMapTo<ContestForActivityServiceModel>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsOnlineExam { get; set; }

    public int? CategoryId { get; set; }

    public bool IsVisible { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public ContestType Type { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }
}