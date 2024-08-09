namespace OJS.Services.Infrastructure.Models;

using System;

public class ExceptionCacheEntry
{
    public string Value { get; set; } = null!;

    public DateTime LastEmailSentTime { get; set; }
}