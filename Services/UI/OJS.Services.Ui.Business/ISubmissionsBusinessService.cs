namespace OJS.Services.Ui.Business
{
    using OJS.Common.Enumerations;
    using OJS.Services.Common.Models.Submissions;
    using OJS.Services.Ui.Models.Submissions;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Services.Common.Models.Pagination;

    public interface ISubmissionsBusinessService : IService
    {
        Task<ServiceResult<VoidResult>> Submit(SubmitSubmissionServiceModel model);

        Task Retest(int submissionId, bool verbosely = false);

        Task<SubmissionDetailsServiceModel> GetDetailsById(int submissionId);

        Task<PagedResult<TServiceModel>> GetByUsername<TServiceModel>(
            string? username,
            PaginationRequestModel requestModel);

        Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult);

        Task<ServiceResult<PagedResult<SubmissionForSubmitSummaryServiceModel>>> GetUserSubmissionsByProblem(int problemId, bool isOfficial, PaginationRequestModel requestModel);

        Task<int> GetTotalCount();

        Task<PagedResult<TServiceModel>> GetSubmissions<TServiceModel>(
            SubmissionStatus status,
            PaginationRequestModel requestModel);

        Task<SubmissionFileDownloadServiceModel> GetSubmissionFile(int submissionId);

        Task<Dictionary<SubmissionProcessingState, int>> GetAllUnprocessedCount();

        string GetLogFilePath(int submissionId);
    }
}