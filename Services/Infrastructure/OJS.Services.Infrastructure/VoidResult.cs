namespace OJS.Services.Infrastructure;

public sealed class VoidResult
{
    public static readonly VoidResult instance = new();

    private VoidResult() { }
}