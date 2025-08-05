namespace OJS.Workers.ExecutionStrategies.CodeSanitizers;

public class JavaScriptSanitizer : BaseCodeSanitizer
{
    protected override string DoSanitize(string content) => content;

    protected override bool ShouldRemovePathInZipEntry(string normalizedPath)
        => base.ShouldRemovePathInZipEntry(normalizedPath) ||
           normalizedPath
               .TrimEnd('/')
               .Split('/')
               .Any(part => string.Equals(part, "node_modules", StringComparison.Ordinal));
}