namespace OJS.Services.Ui.Data;

using OJS.Data.Models.Submissions;
using OJS.Services.Common.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OJS.Services.Infrastructure.Models;

public interface ISubmissionsDataService : IDataService<Submission>
{
    Task<TServiceModel?> GetSubmissionById<TServiceModel>(int id);

    IQueryable<TServiceModel> GetLatestSubmissions<TServiceModel>(int? limit = null);

    IQueryable<Submission> GetLatestSubmissionsByUserParticipations(IEnumerable<int> userParticipantsIds);

    Task<int> GetSubmissionsPerDayCount();

    IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);

    Task<bool> HasParticipantNotProcessedSubmissionForProblem(int problemId, int participantId);

    Task<bool> HasParticipantNotProcessedSubmissionForContest(int contestId, int participantId);
}