namespace OJS.Services.Administration.Business.Submissions
{
    using System.Threading.Tasks;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Administration.Models.Submissions;
    using OJS.Services.Infrastructure.Models;

    public interface ISubmissionsBusinessService : IAdministrationOperationService<Submission, int, SubmissionAdministrationServiceModel>
    {
        Task<ServiceResult<VoidResult>> Retest(int id, bool verbosely = false);

        Task<SubmissionAdministrationServiceModel> Download(int id);
    }
}