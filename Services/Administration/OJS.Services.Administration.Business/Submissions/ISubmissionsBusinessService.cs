namespace OJS.Services.Administration.Business.Submissions
{
    using System.Threading.Tasks;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Models;
    using OJS.Services.Administration.Models.Submissions;

    public interface ISubmissionsBusinessService : IAdministrationOperationService<Submission, int, SubmissionAdministrationServiceModel>
    {
        Task<ServiceResult> Retest(int id, bool verbosely = false);

        Task<SubmissionAdministrationServiceModel> Download(int id);
    }
}