namespace OJS.Services.Ui.Models.Contests;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Ui.Models.SubmissionTypes;
using SoftUni.AutoMapper.Infrastructure.Models;

public class ContestServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public ContestType Type { get; set; }

    public int? CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public bool IsOnlineExam { get; set; }

    public bool IsExam { get; set; }

    public string ContestPassword { private get; set; } = null!;

    public string PracticePassword { private get; set; } = null!;

    public int OfficialParticipants { get; set; }

    public int PracticeParticipants { get; set; }

    public int ProblemsCount { get; set; }

    public IEnumerable<SubmissionTypeServiceModel> AllowedSubmissionTypes { get; set; } = null!;

    public IEnumerable<ContestProblemServiceModel> Problems { get; set; } = null!;

    public ICollection<LecturerInContestServiceModel> LecturersInContests { get; set; } = null!;

    public ICollection<LecturerInContestCategoryServiceModel> LecturerInContestCategory { get; set; } = null!;

    public IEnumerable<ContestCategoryListViewModel> ParentCategories { get; set; } =
        Enumerable.Empty<ContestCategoryListViewModel>();

    public bool HasContestPassword => this.ContestPassword != null;

    public bool HasPracticePassword => this.PracticePassword != null;

    public bool UserIsAdminOrLecturerInContest { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<Contest, ContestServiceModel>()
            .ForMember(
                d => d.OfficialParticipants,
                opt => opt.MapFrom(s => s.Participants.Count(x => x.IsOfficial)))
            .ForMember(
                d => d.PracticeParticipants,
                opt => opt.MapFrom(s => s.Participants.Count(x => !x.IsOfficial)))
            .ForMember(
                d => d.ProblemsCount,
                opt => opt.MapFrom(s => s.ProblemGroups.SelectMany(pg => pg.Problems).Count(p => !p.IsDeleted)))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(
                d => d.AllowedSubmissionTypes,
                opt =>
                    opt.MapFrom(s => s.ProblemGroups
                        .SelectMany(pg => pg.Problems
                            .SelectMany(p => p.SubmissionTypesInProblems)
                            .Select(st => st.SubmissionType))))
            .ForMember(
                d => d.Problems,
                opt => opt.MapFrom(s =>
                    s.ProblemGroups
                        .SelectMany(pg => pg.Problems)
                        .OrderBy(p => p.ProblemGroup.OrderBy)
                        .ThenBy(p => p.OrderBy)))
            .ForMember(d => d.LecturerInContestCategory, opt => opt.MapFrom(s => s.Category!.LecturersInContestCategories))
            .ForMember(d => d.ParentCategories, opt => opt.Ignore())
            .ForMember(d => d.UserIsAdminOrLecturerInContest, opt => opt.Ignore())
            .ReverseMap();
}