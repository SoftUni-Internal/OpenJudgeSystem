namespace IntegrationTests.Worker;

using OJS.PubSub.Worker.Models.Submissions;

public abstract class BaseSubmissionFactory<TParams>
{
    private int id;

    /// <summary>
    /// A method for getting a submission for test execution.
    /// <param name="strategyParameters">These are the unique properties the strategy's test requires to be changed in order to get the expected result.</param>
    /// </summary>
    public abstract SubmissionForProcessingPubSubModel GetSubmission(TParams strategyParameters);

    /// <summary>
    /// A thread-safe way for creating a unique id for each submission. Based on the 'fixture' implementation, the IDs MUST be unique.
    /// This is faster than using 'lock'.
    /// </summary>
    protected int GetNextId()
        => Interlocked.Increment(ref this.id);
}