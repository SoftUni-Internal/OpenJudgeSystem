namespace OJS.Services.Ui.Models.Participants;

using OJS.Common.Enumerations;
using OJS.Services.Common.Models.Contests;
using System;
using System.Collections.Generic;

public class ParticipantSubmitServiceModel : IParticipantForActivityServiceModel
{
    public int Id { get; set; }

    public bool IsInvalidated { get; set; }

    public bool IsOfficial { get; set; }

    public int ContestId { get; set; }

    public int? ContestCategoryId { get; set; }

    public ContestType ContestType { get; set; }

    public DateTime? LastSubmissionTime { get; set; }

    public DateTime? ParticipationStartTime { get; set; }

    public DateTime? ParticipationEndTime { get; set; }

    public DateTime? ContestStartTime { get; set; }

    public DateTime? ContestEndTime { get; set; }

    public DateTime? ContestPracticeStartTime { get; set; }

    public DateTime? ContestPracticeEndTime { get; set; }

    public int ContestLimitBetweenSubmissions { get; set; }

    public bool ContestAllowParallelSubmissionsInTasks { get; set; }

    public IEnumerable<ProblemForParticipantServiceModel>? Problems { get; set; }
}