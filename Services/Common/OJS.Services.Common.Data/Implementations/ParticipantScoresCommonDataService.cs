﻿namespace OJS.Services.Common.Data.Implementations;

using OJS.Data;
using OJS.Data.Models.Participants;
using System.Collections.Generic;
using System.Linq;

public class ParticipantScoresCommonDataService : DataService<ParticipantScore>, IParticipantScoresCommonDataService
{
    public ParticipantScoresCommonDataService(OjsDbContext db)
        : base(db)
    {
    }

    public IQueryable<ParticipantScore> GetAllByParticipants(IEnumerable<int> participantIds) =>
        this.GetQuery(ps => !ps.Problem.IsDeleted && participantIds.Contains(ps.ParticipantId));
}