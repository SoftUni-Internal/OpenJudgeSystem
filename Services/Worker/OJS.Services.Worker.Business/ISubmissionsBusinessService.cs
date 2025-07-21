namespace OJS.Services.Worker.Business;

using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

public interface ISubmissionsBusinessService : IService
{
    Task<ExecutionResultServiceModel> ExecuteSubmission(SubmissionServiceModel submission, CancellationToken cancellationToken = default);
}