namespace OJS.Workers.Common
{
    public interface ISingleCodeRunResult
    {
        int TimeUsed { get; }

        int MemoryUsed { get; }
    }
}