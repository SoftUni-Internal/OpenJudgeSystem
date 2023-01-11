﻿namespace OJS.Services.Ui.Models.Contests
{

using AutoMapper;
using OJS.Data.Models.Participants;
using OJS.Services.Common.Models;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using System.Linq;

    public class ContestParticipationServiceModel : IMapExplicitly
    {
        public ContestServiceModel Contest { get; set; } = null!;

        public int ParticipantId { get; set; }

        public DateTime? LastSubmissionTime { get; set; }

        public bool ContestIsCompete { get; set; }

        public int? UserSubmissionsTimeLimit { get; set; }

        public double? RemainingTimeInMilliseconds { get; set; }

        public bool ShouldEnterPassword { get; set; }

        public int TotalParticipantsCount { get; set; }

        public ValidationResult ValidationResult { get; set; }

        public  int TotalParticipantsCount { get; set; }

        public int ActiveParticipantsCount { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<Participant, ContestParticipationServiceModel>()
                .ForMember(d => d.Contest, opt => opt.MapFrom(s => s.Contest))
                .ForMember(d => d.LastSubmissionTime, opt => opt.MapFrom(s =>
                    s.Submissions.Any()
                        ? (DateTime?)s.Submissions.Max(x => x.CreatedOn)
                        : null))
                .ForMember(d => d.RemainingTimeInMilliseconds, opt => opt.MapFrom(s =>
                    s.ParticipationEndTime.HasValue
                        ? (s.ParticipationEndTime.Value - DateTime.Now).TotalMilliseconds
                        : 0))
                .ForMember(d => d.TotalParticipantsCount, opt => opt.MapFrom(s =>
                    s.Contest.Participants.Count))
                .ForMember(d => d.ActiveParticipantsCount, opt => opt.MapFrom(s =>
                    s.Contest.Participants.Count(x => x.ParticipationStartTime <= DateTime.Now && DateTime.Now < x.ParticipationEndTime)))
                .ForAllOtherMembers(opt => opt.Ignore());
    }
}