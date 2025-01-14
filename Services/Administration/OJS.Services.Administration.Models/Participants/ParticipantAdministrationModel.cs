﻿namespace OJS.Services.Administration.Models.Participants;

using AutoMapper;
using OJS.Data.Models.Participants;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ParticipantAdministrationModel : BaseAdministrationModel<int>, IMapExplicitly
{
    public int ContestId { get; set; }

    public string? ContestName { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public bool IsOfficial { get; set; }

    public bool IsInvalidated { get; set; }

    public DateTime? ParticipationStartTime { get; set; }

    public DateTime? ParticipationEndTime { get; set; }

    public DateTime? LastSubmissionTime { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
    {
        configuration.CreateMap<Participant, ParticipantAdministrationModel>()
            .ForMember(pam => pam.UserName, opt
                => opt.MapFrom(p => p.User.UserName))
            .ForMember(pam => pam.OperationType, opt
                => opt.Ignore());

        configuration.CreateMap<ParticipantAdministrationModel, Participant>()
            .ForMember(p => p.CreatedOn, opt
                => opt.Ignore())
            .ForMember(p => p.ModifiedOn, opt
                => opt.Ignore())
            .ForMember(p => p.Contest, opt
                => opt.Ignore())
            .ForMember(p => p.User, opt
                => opt.Ignore())
            .ForMember(p => p.ParticipationStartTime, opt
                => opt.Ignore())
            .ForMember(p => p.ParticipationEndTime, opt
                => opt.Ignore())
            .ForMember(p => p.IsInvalidated, opt
                => opt.Ignore())
            .ForMember(p => p.TotalScoreSnapshot, opt
                => opt.Ignore())
            .ForMember(p => p.TotalScoreSnapshotModifiedOn, opt
                => opt.Ignore())
            .ForMember(p => p.Submissions, opt
                => opt.Ignore())
            .ForMember(p => p.Scores, opt
                => opt.Ignore())
            .ForMember(p => p.ProblemsForParticipants, opt
                => opt.Ignore());
    }
}