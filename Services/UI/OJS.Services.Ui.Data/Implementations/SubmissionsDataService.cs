namespace OJS.Services.Ui.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Common.Extensions;
using OJS.Data;
using OJS.Data.Models.Submissions;
using OJS.Services.Common.Data.Implementations;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SubmissionsDataService(OjsDbContext db) : DataService<Submission>(db), ISubmissionsDataService
{
    public Task<TServiceModel?> GetSubmissionById<TServiceModel>(int id)
        => this.GetByIdQuery(id)
            .AsNoTracking()
            .MapCollection<TServiceModel>()
            .FirstOrDefaultAsync();

    public IQueryable<Submission> GetLatestSubmissions(int? limit = null)
        => this.GetQuery(
                orderBy: s => s.Id,
                descending: true,
                take: limit);

    public IQueryable<Submission> GetLatestSubmissionsByUserParticipations(IEnumerable<int> userParticipantsIds)
        => this.GetQuery(
            filter: s => !s.IsDeleted && userParticipantsIds.Contains(s.ParticipantId),
            orderBy: s => s.Id,
            descending: true);

    public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId)
        => this.GetQuery(
            filter: s => s.ParticipantId == participantId && s.ProblemId == problemId,
            orderBy: q => q.CreatedOn,
            descending: true);

    public Task<bool> HasParticipantNotProcessedSubmissionForProblem(int problemId, int participantId)
        => this.Exists(s => s.ProblemId == problemId && s.ParticipantId == participantId && !s.Processed);

    public Task<bool> HasParticipantNotProcessedSubmissionForContest(int contestId, int participantId)
        => this.Exists(s =>
            s.Problem.ProblemGroup.ContestId == contestId &&
            s.ParticipantId == participantId && !s.Processed);

    public async Task<int> GetSubmissionsPerDayCount()
        => await this.GetQuery().AnyAsync()
            ? await this.GetQuery().GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.DayOfYear })
                .Select(x => x.Count())
                .AverageAsync()
                .ToInt()
            : 0;
}