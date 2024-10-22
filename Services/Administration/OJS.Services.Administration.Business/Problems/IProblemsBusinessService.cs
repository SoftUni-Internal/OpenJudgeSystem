namespace OJS.Services.Administration.Business.Problems;

using OJS.Data.Models.Problems;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Common.Models;
using System.Threading.Tasks;

public interface IProblemsBusinessService : IAdministrationOperationService<Problem, int, ProblemAdministrationModel>
{
    Task RetestById(int id);

    Task DeleteByContest(int contestId);

    Task<ServiceResult> CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId);

    Task<bool> UserHasProblemPermissions(int problemId, string? userId, bool isUserAdmin);

    Task ReevaluateProblemsOrder(int contestId);
}