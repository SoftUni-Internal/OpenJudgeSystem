namespace OJS.Services.Common.Data;

using OJS.Data.Models.Submissions;
using System.Linq;

public interface ISubmissionsCommonDataService : IDataService<Submission>
{
    IQueryable<Submission> GetAllPending(int? fromMinutesAgo = null);

    IQueryable<Submission> GetAllEnqueued();

    IQueryable<Submission> GetAllProcessing();

    IQueryable<Submission> GetAllFaulted();
}