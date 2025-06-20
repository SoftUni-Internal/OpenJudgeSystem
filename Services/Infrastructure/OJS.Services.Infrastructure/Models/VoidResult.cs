namespace OJS.Services.Infrastructure.Models;

public sealed class VoidResult
{
    public static readonly VoidResult instance = new();

    private VoidResult() { }
}