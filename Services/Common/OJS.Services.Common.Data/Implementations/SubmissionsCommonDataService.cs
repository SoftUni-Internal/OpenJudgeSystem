namespace OJS.Services.Common.Data.Implementations;

using OJS.Data;
using OJS.Data.Models.Submissions;
using System.Linq;

public class SubmissionsCommonDataService : DataService<Submission>, ISubmissionsCommonDataService
{
    private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingCommonDataService;

    public SubmissionsCommonDataService(
        OjsDbContext db,
        ISubmissionsForProcessingCommonDataService submissionsForProcessingCommonDataService)
        : base(db)
        => this.submissionsForProcessingCommonDataService = submissionsForProcessingCommonDataService;

    public IQueryable<Submission> GetAllPending(int? fromMinutesAgo = null)
        => this.GetFromSubmissionsForProcessing(
            this.submissionsForProcessingCommonDataService.GetAllPending(fromMinutesAgo));

    public IQueryable<Submission> GetAllEnqueued()
        => this.GetFromSubmissionsForProcessing(
            this.submissionsForProcessingCommonDataService.GetAllEnqueued());

    public IQueryable<Submission> GetAllProcessing()
        => this.GetFromSubmissionsForProcessing(
            this.submissionsForProcessingCommonDataService.GetAllProcessing());

    public IQueryable<Submission> GetAllFaulted()
        => this.GetFromSubmissionsForProcessing(
            this.submissionsForProcessingCommonDataService.GetAllFaulted());

    private IQueryable<Submission> GetFromSubmissionsForProcessing(
        IQueryable<SubmissionForProcessing> submissionsForProcessing)
        => submissionsForProcessing
            .Join(
                this.GetQuery(),
                sfp => sfp.SubmissionId,
                submission => submission.Id,
                (_, submission) => submission)
            .Where(submission => !submission.IsDeleted);
}