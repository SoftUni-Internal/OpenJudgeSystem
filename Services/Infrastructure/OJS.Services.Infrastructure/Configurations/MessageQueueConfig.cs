namespace OJS.Services.Infrastructure.Configurations;

using System.ComponentModel.DataAnnotations;

public class MessageQueueConfig : BaseConfig
{
    public override string SectionName => "MessageQueue";

    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    public string VirtualHost { get; set; } = string.Empty;

    [Required]
    public string User { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public int RetryCount { get; set; } = 5;

    public int RetryInterval { get; set; } = 200;

    public int? PrefetchCount { get; set; }

    public int TimeoutInSeconds { get; set; } = 600;
}