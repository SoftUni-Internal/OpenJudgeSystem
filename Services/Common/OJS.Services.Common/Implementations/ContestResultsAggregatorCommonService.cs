namespace OJS.Services.Common.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Services.Common.Data;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Participants;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Models.Contests.Results;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

public class ContestResultsAggregatorCommonService : IContestResultsAggregatorCommonService
{
    private readonly IContestsActivityService activityService;
    private readonly IParticipantScoresCommonDataService participantScoresCommonDataService;
    private readonly IParticipantsCommonDataService participantsCommonData;

    public ContestResultsAggregatorCommonService(
        IContestsActivityService activityService,
        IParticipantScoresCommonDataService participantScoresCommonDataService,
        IParticipantsCommonDataService participantsCommonData)
    {
        this.activityService = activityService;
        this.participantScoresCommonDataService = participantScoresCommonDataService;
        this.participantsCommonData = participantsCommonData;
    }

    public ContestResultsViewModel GetContestResults(ContestResultsModel contestResultsModel)
    {
        var contestActivityEntity = this.activityService
            .GetContestActivity(contestResultsModel.Contest.Map<ContestForActivityServiceModel>());

        var contestResults = contestResultsModel.Map<ContestResultsViewModel>();
        contestResults.Id = contestResultsModel.Contest.Id;

        contestResults.ContestCanBeCompeted = contestActivityEntity.CanBeCompeted;
        contestResults.ContestCanBePracticed = contestActivityEntity.CanBePracticed;

        var problems = contestResultsModel.Contest.ProblemGroups
            .SelectMany(pg => pg.Problems)
            .AsQueryable()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.OrderBy)
            .ThenBy(p => p.Name)
            .Select(ContestProblemListViewModel.FromProblem)
            .ToList();

        var totalParticipantsCount = contestResultsModel.TotalResultsCount
                                     ?? this.participantsCommonData
                                         .GetAllByContestAndIsOfficial(
                                             contestResultsModel.Contest.Id,
                                             contestResultsModel.Official)
                                         .Count();

        // Get the requested participants without their problem results.
        // Splitting the queries improves performance and avoids unexpected results from joins with Scores.
        var participants = this.GetParticipantsPage(
                contestResultsModel.Contest,
                contestResultsModel.Official,
                contestResultsModel.Page,
                contestResultsModel.ItemsInPage)
            .Select(ParticipantResultViewModel.FromParticipant)
            .ToList();

        // Get the ParticipantScores with another query and map problem results for each participant.
        var participantScores = this.participantScoresCommonDataService
            .GetAllByParticipants(participants.Select(p => p.Id))
            .Include(x => x.Participant)
            .Include(x => x.Submission)
            .Include(x => x.Problem)
            .AsNoTracking();

        IEnumerable<ProblemResultPairViewModel> problemResults;

        if (contestResultsModel.IsFullResults)
        {
            problemResults = participantScores
                .Select(ProblemResultPairViewModel.FromParticipantScoreAsFullResult)
                .ToList();
        }
        else if (contestResultsModel.IsExportResults)
        {
            problemResults = participantScores
                .Select(ProblemResultPairViewModel.FromParticipantScoreAsExportResult)
                .ToList();
        }
        else
        {
            problemResults = participantScores
                .Select(ProblemResultPairViewModel.FromParticipantScoreAsSimpleResult)
                .ToList();
        }

        participants.ForEach(p =>
            p.ProblemResults = problemResults.Where(pr => pr.ParticipantId == p.Id));

        var results = new StaticPagedList<ParticipantResultViewModel>(
            participants,
            contestResultsModel.Page,
            contestResultsModel.ItemsInPage,
            totalParticipantsCount);

        contestResults.Problems = problems;
        contestResults.Results = results;

        return contestResults;
    }

    /// <summary>
    /// Gets IQueryable results with one page of participants, ordered by top score.
    /// </summary>
    private IQueryable<Participant> GetParticipantsPage(Contest contest, bool official, int page, int itemsInPage) =>
        this.participantsCommonData
            .GetAllByContestAndIsOfficial(contest.Id, official)
            .AsNoTracking()
            .OrderByDescending(p => p.TotalScoreSnapshot)
            .ThenBy(p => p.TotalScoreSnapshotModifiedOn)
            .Skip((page - 1) * itemsInPage)
            .Take(itemsInPage);
}