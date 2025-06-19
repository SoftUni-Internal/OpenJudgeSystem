namespace OJS.Services.Administration.Business.Problems;

using System.IO;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Common.Models;
using System.Threading.Tasks;

public interface IProblemsBusinessService : IAdministrationOperationService<Problem, int, ProblemAdministrationModel>
{
    Task<ProblemRetestValidationModel> ValidateRetest(int id);

    Task RetestById(int id);

    Task DeleteByContest(int contestId);

    Task<ServiceResult<VoidResult>> CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId);

    Task<(MemoryStream outputStream, string zipFileName)> DownloadAdditionalFiles(int problemId);
}