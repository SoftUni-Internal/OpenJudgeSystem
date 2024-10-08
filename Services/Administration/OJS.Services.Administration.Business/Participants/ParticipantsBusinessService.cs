﻿namespace OJS.Services.Administration.Business.Participants;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Participants;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Participants;
using OJS.Services.Infrastructure.Extensions;
using System.Linq;
using System.Threading.Tasks;

public class ParticipantsBusinessService : AdministrationOperationService<Participant, int, ParticipantAdministrationModel>, IParticipantsBusinessService
{
    private readonly IParticipantsDataService participantsData;
    private readonly IParticipantScoresDataService scoresDataService;

    public ParticipantsBusinessService(
        IParticipantsDataService participantsData,
        IParticipantScoresDataService scoresDataService)
    {
        this.participantsData = participantsData;
        this.scoresDataService = scoresDataService;
    }

    public override async Task<ParticipantAdministrationModel> Create(ParticipantAdministrationModel model)
    {
        var participant = model.Map<Participant>();

        await this.participantsData.Add(participant);
        await this.participantsData.SaveChanges();

        return model;
    }

    public override async Task Delete(int id)
    {
        await this.participantsData.DeleteById(id);
        await this.participantsData.SaveChanges();
    }

    public async Task UpdateTotalScoreSnapshotOfParticipants()
        => await this.participantsData.UpdateTotalScoreSnapshot();

    public async Task RemoveDuplicateParticipantScores()
    {
        var duplicateGroups = await this.scoresDataService
            .GetAll()
            .GroupBy(ps => new { ps.IsOfficial, ps.ProblemId, ps.ParticipantId })
            .Where(psGroup => psGroup.Count() > 1)
            .Select(psGroup => new
            {
                GroupKey = psGroup.Key,
                ScoresToRemove = psGroup
                    .OrderByDescending(ps => ps.Points)
                    .Skip(1),
            })
            .ToListAsync();

        var participantScoresToRemove = duplicateGroups
            .SelectMany(group => group.ScoresToRemove)
            .ToList();

        await this.scoresDataService.Delete(participantScoresToRemove);
    }

    public IQueryable<Participant> GetByContest(int contestId)
        => this.participantsData.GetAllByContest(contestId);
}