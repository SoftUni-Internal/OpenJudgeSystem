namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Data.Models.Participants;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using System.Linq;

public class ContestParticipationServiceModel : IMapExplicitly
{
    public ContestServiceModel? Contest { get; set; } = null!;

    public bool IsRegisteredParticipant { get; set; }

    public bool IsInvalidated { get; set; }

    public bool IsActiveParticipant { get; set; }

    public int ParticipantId { get; set; }

    public DateTime? LastSubmissionTime { get; set; }

    public int? UserSubmissionsTimeLimit { get; set; }

    public DateTime? EndDateTimeForParticipantOrContest { get; set; }

    /// <summary>
    /// Gets or sets the count of participants in the contest taking into consideration if it is compete or practice.
    /// </summary>
    public int ParticipantsCount { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Participant, ContestParticipationServiceModel>()
            .ForMember(d => d.IsInvalidated, opt => opt.MapFrom(s => s.IsInvalidated))
            .ForMember(d => d.Contest, opt => opt.Ignore())
            .ForMember(d => d.EndDateTimeForParticipantOrContest, opt => opt.Ignore())
            .ForAllOtherMembers(opt => opt.Ignore());
}