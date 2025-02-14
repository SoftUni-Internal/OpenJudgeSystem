namespace OJS.Workers.Common.Extensions;

public static class ExecutorExtensions
{
    public static IExecutor KeepEnvironmentVariables(
        this IExecutor executor,
        params string[] environmentVariableNames)
    {
        foreach (var key in environmentVariableNames)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (value != null)
            {
                executor.EnvironmentVariables.Add(key, value);
            }
        }

        return executor;
    }
}