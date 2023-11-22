namespace OJS.Services.Ui.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Participants;
using System.Linq;
using OJS.Services.Common.Data.Implementations;

public class ParticipantsCommonDataService : DataService<Participant>, IParticipantsCommonDataService
{
    public ParticipantsCommonDataService(DbContext db)
        : base(db)
    {
    }

    public IQueryable<Participant> GetAllByContest(int contestId)
        => this.DbSet
            .Where(p => p.ContestId == contestId);

    public IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial)
        => this.GetAllByContest(contestId)
            .Where(p => p.IsOfficial == isOfficial);

    public IQueryable<Participant> GetAllWithProblemsScoresAndSubmissionsByContestAndIsOfficial(int contestId, bool isOfficial)
        => this.GetAllByContestAndIsOfficial(contestId, isOfficial)
            .Include(p => p.ProblemsForParticipants)
            .Include(p => p.Scores)
                .ThenInclude(s => s.Problem)
                    .ThenInclude(p => p.ProblemGroup)
            .Include(p => p.Scores)
                .ThenInclude(s => s.Submission)
                    .ThenInclude(s => s!.SubmissionType);
}