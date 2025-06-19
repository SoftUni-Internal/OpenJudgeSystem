namespace OJS.Services.Common.Models;

public sealed class VoidResult
{
    public static readonly VoidResult instance = new();

    private VoidResult() { }
}