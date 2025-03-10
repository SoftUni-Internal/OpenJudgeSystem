namespace OJS.Services.Ui.Business.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Problems;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Contests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ParticipantsBusinessService : IParticipantsBusinessService
{
    private readonly IParticipantsDataService participantsData;
    private readonly IDatesService datesService;
    private readonly IProblemGroupsDataService problemGroupsData;

    public ParticipantsBusinessService(
        IParticipantsDataService participantsData,
        IDatesService datesService,
        IProblemGroupsDataService problemGroupsData)
    {
        this.participantsData = participantsData;
        this.datesService = datesService;
        this.problemGroupsData = problemGroupsData;
    }

    public async Task<Participant> CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
        ContestRegistrationDetailsServiceModel contest,
        string userId,
        bool isOfficial,
        bool isAdminOrLecturerInContest)
    {
        var participant = new Participant(contest.Id, userId, isOfficial);

        if (isOfficial)
        {
            if (contest.IsOnlineExam)
            {
                var utcNow = DateTime.SpecifyKind(this.datesService.GetUtcNow(), DateTimeKind.Unspecified);
                participant.ParticipationStartTime = utcNow;
                participant.ParticipationEndTime = utcNow + contest.Duration;
            }

            if (!isAdminOrLecturerInContest && contest.IsWithRandomTasks)
            {
                var problemGroups = await this.problemGroupsData
                    .GetAllByContest(contest.Id)
                    .Include(pg => pg.Problems)
                    .ToListAsync();

                AssignRandomProblemsToParticipant(participant, problemGroups);
            }
        }

        await this.participantsData.Add(participant);
        await this.participantsData.SaveChanges();

        return participant;
    }

    private static void AssignRandomProblemsToParticipant(Participant participant, List<ProblemGroup> problemGroups)
    {
        var random = new Random();

        foreach (var problemGroup in problemGroups.Where(pg => pg.Problems.Any(p => !p.IsDeleted)))
        {
            var problemsInGroup = problemGroup.Problems.Where(p => !p.IsDeleted).ToList();
            if (problemsInGroup.Count != 0)
            {
                var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                participant.ProblemsForParticipants.Add(new ProblemForParticipant
                {
                    Participant = participant,
                    Problem = randomProblem,
                });
            }
        }
    }
}