namespace OJS.Services.Ui.Business.Implementations;

using OJS.Services.Common;
using System.Threading.Tasks;

public class RecurringBackgroundJobsBusinessService : IRecurringBackgroundJobsBusinessService
{
    // Method is executed by administration implementation
    public Task<object> EnqueuePendingSubmissions() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> DeleteProcessedSubmissions() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> UpdateTotalScoreSnapshotOfParticipants() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> RemoveParticipantMultipleScores() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> NormalizeAllPointsThatExceedAllowedLimit() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> ArchiveOldSubmissionsDailyBatch() => throw new System.NotImplementedException();

    // Method is executed by administration implementation
    public Task<object> HardDeleteArchivedSubmissions() => throw new System.NotImplementedException();
}