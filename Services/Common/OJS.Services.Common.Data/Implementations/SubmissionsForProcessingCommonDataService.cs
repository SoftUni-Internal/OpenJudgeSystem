namespace OJS.Services.Common.Data.Implementations;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Data;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OJS.Common.Enumerations.SubmissionProcessingState;

public class SubmissionsForProcessingCommonDataService(
    OjsDbContext submissionsForProcessing,
    IDatesService dates,
    ILogger<SubmissionsForProcessingCommonDataService> logger)
    : DataService<SubmissionForProcessing>(submissionsForProcessing), ISubmissionsForProcessingCommonDataService
{
    public Task<SubmissionForProcessing?> GetBySubmission(int submissionId)
        => this.GetQuery(s => s.SubmissionId == submissionId)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

    public IQueryable<SubmissionForProcessing> GetAllPending(int? fromMinutesAgo)
        => fromMinutesAgo is null
            ? this.GetQuery(sfp => sfp.State == Pending)
            : this.GetQuery(sfp => sfp.State == Pending
                && (sfp.ModifiedOn ?? sfp.CreatedOn) < dates.GetUtcNow().AddMinutes(-fromMinutesAgo.Value));

    public IQueryable<SubmissionForProcessing> GetAllEnqueued()
        => this.GetQuery(sfp => sfp.State == Enqueued);

    public IQueryable<SubmissionForProcessing> GetAllUnprocessed()
        => this.GetQuery(sfp => sfp.State != Processed && sfp.State != Faulted);

    public IQueryable<SubmissionForProcessing> GetAllProcessing()
        => this.GetQuery(sfp => sfp.State == Processing);

    public IQueryable<SubmissionForProcessing> GetAllFaulted()
        => this.GetQuery(sfp => sfp.State == Faulted);

    public IQueryable<SubmissionForProcessing> GetAllProcessed(int fromMinutesAgo)
        => this.GetQuery(sfp => sfp.State == Processed
            && sfp.ProcessedAt < dates.GetUtcNowOffset().AddMinutes(-fromMinutesAgo));

    public async Task<SubmissionForProcessing> Add(int submissionId)
    {
        var submissionForProcessing = new SubmissionForProcessing
        {
            SubmissionId = submissionId,
            State = Pending,
        };

        await this.Add(submissionForProcessing);

        return submissionForProcessing;
    }

    public async Task<SubmissionForProcessing> AddOrUpdate(int submissionId)
    {
        var entity = await this.GetBySubmission(submissionId);

        if (entity is null)
        {
            entity = await this.Add(submissionId);
        }
        else
        {
            entity.State = Pending;

            this.Update(entity);
        }

        return entity;
    }

    public async Task AddOrUpdateMany(ICollection<int> submissionIds)
    {
        var newSubmissionsForProcessing = submissionIds
            .Select(sId => new SubmissionForProcessing
            {
                SubmissionId = sId,
            });

        await submissionIds
            .Chunk(GlobalConstants.BatchOperationsChunkSize)
            .ForEachSequential(async chunk =>
            {
                this.Delete(sfp => chunk.Contains(sfp.SubmissionId));
                await this.SaveChanges();
            });

        await this.AddMany(newSubmissionsForProcessing);
        await this.SaveChanges();
    }

    public async Task RemoveBySubmission(int submissionId)
    {
        var submissionForProcessing = await this.GetBySubmission(submissionId);

        if (submissionForProcessing != null)
        {
            await this.DeleteById(submissionForProcessing.Id);
            await this.SaveChanges();
        }
    }

    public async Task SetProcessingState(
        SubmissionForProcessing submissionForProcessing,
        SubmissionProcessingState state,
        DateTimeOffset? stateChangedAt = null)
    {
        var updateState = true;
        stateChangedAt ??= dates.GetUtcNowOffset();

        switch (state)
        {
            case Enqueued:
                submissionForProcessing.EnqueuedAt = stateChangedAt;
                updateState = submissionForProcessing.State == Pending;
                break;
            case Processing:
                submissionForProcessing.ProcessingStartedAt = stateChangedAt;
                updateState = submissionForProcessing.State is Enqueued or Pending;
                break;
            case Processed:
                submissionForProcessing.ProcessedAt = stateChangedAt;
                break;
            case Faulted:
                break;
            case Pending:
            case Invalid:
                throw new ArgumentException($"Not allowed to set processing state to {state}.", nameof(state));
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Processing state is not in the valid range.");
        }

        if (updateState)
        {
            submissionForProcessing.State = state;
        }
        else
        {
            logger.LogSubmissionProcessingStateNotUpdated(
                submissionForProcessing.SubmissionId,
                submissionForProcessing.State.ToString(),
                state.ToString());
        }

        this.Update(submissionForProcessing);
        await this.SaveChanges();
    }

    public async Task<int> MarkMultipleEnqueued(ICollection<int> submissionIds, DateTimeOffset? enqueuedAt = null)
    {
        var enqueuedAtDate = enqueuedAt ?? dates.GetUtcNowOffset();
        return await this.GetQuery(sfp => submissionIds.Contains(sfp.SubmissionId))
            .IgnoreQueryFilters()
            .UpdateFromQueryAsync(sfp => new SubmissionForProcessing
            {
                State = Enqueued,
                EnqueuedAt = enqueuedAtDate,
            });
    }
}