namespace OJS.Services.Common.Data;

using OJS.Common.Enumerations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OJS.Data.Models.Submissions;
using System;

public interface ISubmissionsForProcessingCommonDataService : IDataService<SubmissionForProcessing>
{
    Task<SubmissionForProcessing?> GetBySubmission(int submissionId);

    IQueryable<SubmissionForProcessing> GetAllPending(int? fromMinutesAgo);

    IQueryable<SubmissionForProcessing> GetAllEnqueued();

    IQueryable<SubmissionForProcessing> GetAllUnprocessed();

    IQueryable<SubmissionForProcessing> GetAllProcessing();

    IQueryable<SubmissionForProcessing> GetAllFaulted();

    IQueryable<SubmissionForProcessing> GetAllProcessed(int fromMinutesAgo);

    Task<SubmissionForProcessing> Add(int submissionId);

    Task<SubmissionForProcessing> AddOrUpdate(int submissionId);

    Task AddOrUpdateMany(ICollection<int> submissionIds);

    Task RemoveBySubmission(int submissionId);

    Task<int> MarkMultipleEnqueued(ICollection<int> submissionIds, DateTimeOffset? enqueuedAt = null);

    /// <summary>
    /// <para>Sets the processing state of the submission in the submissions for processing table.</para>
    /// <para>
    /// Sometimes race conditions may occur, as processing is done in parallel,
    /// so the state is set only if the new state is logically after the current one.
    /// In such cases, we update only the date of the change event, but not the state itself,
    /// as the submission is already in a more advanced state, and we don't want to revert it,
    /// except for the <see cref="SubmissionProcessingState.Processed"/> state,
    /// which is final and ensures that the submission is not processed again.
    /// </para>
    /// <para><see cref="SubmissionProcessingState.Pending"/> and <see cref="SubmissionProcessingState.Invalid"/>
    /// states are not allowed to be set directly.</para>
    /// </summary>
    /// <param name="submissionForProcessing">The submission for processing to update.</param>
    /// <param name="state">The new state to set.</param>
    /// <param name="stateChangedAt">The date of the state change. If not provided, the current date is used.</param>
    Task SetProcessingState(
        SubmissionForProcessing submissionForProcessing,
        SubmissionProcessingState state,
        DateTimeOffset? stateChangedAt = null);
}