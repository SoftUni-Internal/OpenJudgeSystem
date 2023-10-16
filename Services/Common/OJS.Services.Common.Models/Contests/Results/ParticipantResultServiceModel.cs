namespace OJS.Services.Common.Models.Contests.Results;

using OJS.Data.Models.Participants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class ParticipantResultServiceModel
{
    public string? ParticipantUsername { get; set; }

    public string? ParticipantFirstName { get; set; }

    public string? ParticipantLastName { get; set; }

    public IEnumerable<ProblemResultPairServiceModel> ProblemResults { get; set; }
        = Enumerable.Empty<ProblemResultPairServiceModel>();

    public string ParticipantFullName => $"{this.ParticipantFirstName?.Trim()} {this.ParticipantLastName?.Trim()}";

    public int Total => this.ProblemResults
        .Where(pr => pr.ShowResult)
        .Sum(pr => pr.BestSubmission.Points);

    public int AdminTotal => this.ProblemResults
        .Sum(pr => pr.BestSubmission.Points);

    public int ExportTotal => this.ProblemResults
        .Where(pr => pr.ShowResult && !pr.IsExcludedFromHomework)
        .Sum(pr => pr.BestSubmission.Points);

    public IEnumerable<int> ParticipantProblemIds { get; set; } = Enumerable.Empty<int>();

    public static Expression<Func<Participant, ParticipantResultServiceModel>> FromParticipantAsSimpleResultByContest(int contestId) =>
        participant => new ParticipantResultServiceModel
        {
            ParticipantUsername = participant.User.UserName,
            ParticipantFirstName = participant.User.UserSettings.FirstName,
            ParticipantLastName = participant.User.UserSettings.LastName,
            ParticipantProblemIds = participant.ProblemsForParticipants.Select(p => p.ProblemId),
            ProblemResults = participant.Scores
                .Where(sc => !sc.Problem.IsDeleted && sc.Problem.ProblemGroup.ContestId == contestId)
                .AsQueryable()
                .Select(ProblemResultPairServiceModel.FromParticipantScoreAsSimpleResult)
                .ToList(),
        };

    public static Expression<Func<Participant, ParticipantResultServiceModel>> FromParticipantAsFullResultByContest(int contestId) =>
        participant => new ParticipantResultServiceModel
        {
            ParticipantUsername = participant.User.UserName,
            ParticipantFirstName = participant.User.UserSettings.FirstName,
            ParticipantLastName = participant.User.UserSettings.LastName,
            ProblemResults = participant.Scores
                .Where(sc => !sc.Problem.IsDeleted && sc.Problem.ProblemGroup.ContestId == contestId)
                .AsQueryable()
                .Select(ProblemResultPairServiceModel.FromParticipantScoreAsFullResult)
                .ToList(),
        };

    public static Expression<Func<Participant, ParticipantResultServiceModel>> FromParticipantAsExportResultByContest(int contestId) =>
        participant => new ParticipantResultServiceModel
        {
            ParticipantUsername = participant.User.UserName,
            ParticipantFirstName = participant.User.UserSettings.FirstName,
            ParticipantLastName = participant.User.UserSettings.LastName,
            ProblemResults = participant.Scores
                .Where(sc => !sc.Problem.IsDeleted && sc.Problem.ProblemGroup.ContestId == contestId)
                .AsQueryable()
                .Select(ProblemResultPairServiceModel.FromParticipantScoreAsExportResult)
                .ToList(),
        };
}