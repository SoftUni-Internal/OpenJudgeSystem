namespace OJS.Services.Administration.Business.Contests;

using OJS.Data.Models.Contests;
using OJS.Services.Administration.Models;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Administration.Models.Submissions;
using OJS.Services.Common.Models.Files;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IContestsBusinessService : IAdministrationOperationService<Contest, int, ContestAdministrationModel>
{
    Task<IEnumerable<LecturerInContestActionsModel>> GetForLecturerInContest(string userId);

    Task<bool> UserHasContestPermissions(int contestId, string? userId, bool isUserAdmin);

    Task<FileResponseModel> ExportResults(ContestResultsExportRequestModel model);

    Task<FileResponseModel> DownloadSubmissions(DownloadSubmissionsModel model);

    Task<ContestLegacyExportServiceModel> Export(int id);

    Task<IEnumerable<int>> GetExistingIds(IEnumerable<int> ids);

    Task<ContestActivityModel> GetContestActivity(int contestId);

    Task TransferParticipantsToPracticeById(int contestId);

    Task AdjustLimitBetweenSubmissions(WorkersBusyRatioServiceModel model);
}