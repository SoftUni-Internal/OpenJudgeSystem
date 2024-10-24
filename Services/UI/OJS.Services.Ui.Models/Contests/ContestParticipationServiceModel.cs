namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Data.Models.Participants;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

public class ContestParticipationServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public ContestServiceModel? Contest { get; set; }

    public bool IsRegisteredParticipant { get; set; }

    public bool IsInvalidated { get; set; }

    public bool IsOfficial { get; set; }

    public bool IsActiveParticipant { get; set; }

    public DateTime? ParticipationStartTime { get; set; }

    public DateTime? ParticipationEndTime { get; set; }

    public int ParticipantId { get; set; }

    public DateTime? LastSubmissionTime { get; set; }

    public int? UserSubmissionsTimeLimit { get; set; }

    public DateTime? EndDateTimeForParticipantOrContest { get; set; }

    public IEnumerable<int> ProblemsForParticipantIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the count of participants in the contest taking into consideration if it is compete or practice.
    /// </summary>
    public int ParticipantsCount { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
    {
        configuration.CreateMap<Participant, ContestParticipationServiceModel>()
            .ForMember(
                d => d.ProblemsForParticipantIds,
                opt => opt.MapFrom(s => s.ProblemsForParticipants.Select(p => p.ProblemId)))
            .ForMember(d => d.IsInvalidated, opt => opt.MapFrom(s => s.IsInvalidated))
            .ForMember(d => d.Contest, opt => opt.Ignore())
            .ForMember(d => d.EndDateTimeForParticipantOrContest, opt => opt.Ignore())
            .ForMember(m => m.IsRegisteredParticipant, opt => opt.Ignore())
            .ForMember(m => m.IsActiveParticipant, opt => opt.Ignore())
            .ForMember(m => m.ParticipantId, opt => opt.Ignore())
            .ForMember(m => m.LastSubmissionTime, opt => opt.Ignore())
            .ForMember(m => m.UserSubmissionsTimeLimit, opt => opt.Ignore())
            .ForMember(m => m.ParticipantsCount, opt => opt.Ignore());

        configuration.CreateMap<ContestParticipationServiceModel, ParticipantForActivityServiceModel>();
    }
}