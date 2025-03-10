namespace OJS.Services.Ui.Business
{
    using OJS.Common.Enumerations;
    using OJS.Services.Common.Models.Submissions;
    using OJS.Services.Ui.Models.Submissions;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using static OJS.Services.Common.Constants.PaginationConstants.Submissions;

    public interface ISubmissionsBusinessService : IService
    {
        Task Submit(SubmitSubmissionServiceModel model);

        Task Retest(int submissionId, bool verbosely = false);

        Task<SubmissionDetailsServiceModel> GetDetailsById(int submissionId);

        Task<PagedResult<TServiceModel>> GetByUsername<TServiceModel>(
            string? username,
            int page,
            int itemsInPage = DefaultSubmissionsPerPage);

        Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult);

        Task<PagedResult<SubmissionForSubmitSummaryServiceModel>> GetUserSubmissionsByProblem(int problemId, bool isOfficial, int page);

        Task<int> GetTotalCount();

        Task<PagedResult<TServiceModel>> GetSubmissions<TServiceModel>(
            SubmissionStatus status,
            int page,
            int itemsPerPage = DefaultSubmissionsPerPage);

        Task<SubmissionFileDownloadServiceModel> GetSubmissionFile(int submissionId);

        Task<Dictionary<SubmissionProcessingState, int>> GetAllUnprocessedCount();

        string GetLogFilePath(int submissionId);
    }
}