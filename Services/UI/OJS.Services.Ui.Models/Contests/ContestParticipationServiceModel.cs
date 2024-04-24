namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Data.Models.Participants;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using System.Linq;

public class ContestParticipationServiceModel : IMapExplicitly
{
    public ContestServiceModel? Contest { get; set; } = null!;

    public bool IsRegisteredParticipant { get; set; }

    public bool IsActiveParticipant { get; set; }

    public int ParticipantId { get; set; }

    public DateTime? LastSubmissionTime { get; set; }

    public bool ContestIsCompete { get; set; }

    public int? UserSubmissionsTimeLimit { get; set; }

    public DateTime? EndDateTimeForParticipantOrContest { get; set; }

    /// <summary>
    /// Gets or sets the count of participants in the contest taking into consideration if it is compete or practice.
    /// </summary>
    public int ParticipantsCount { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Participant, ContestParticipationServiceModel>()
            .ForMember(d => d.Contest, opt => opt.MapFrom(s => s.Contest))
            .ForMember(d => d.EndDateTimeForParticipantOrContest, opt => opt.MapFrom(s =>
                s.ParticipationEndTime.HasValue
                ? s.ParticipationEndTime
                : s.Contest.EndTime.HasValue && s.Contest.EndTime >= DateTime.UtcNow
                    ? s.Contest.EndTime
                    : s.Contest.PracticeEndTime.HasValue
                        ? s.Contest.PracticeEndTime
                        : null))
            .ForAllOtherMembers(opt => opt.Ignore());
}