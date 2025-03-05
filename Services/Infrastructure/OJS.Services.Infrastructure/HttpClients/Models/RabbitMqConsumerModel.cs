namespace OJS.Services.Infrastructure.HttpClients.Models;

using System.Text.Json.Serialization;

public class RabbitMqConsumerModel
{
    /// <summary>
    /// Is the consumer active. If the consumer is not active, it will not receive any messages.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// The number of messages that the consumer will prefetch.
    /// </summary>
    [JsonPropertyName("prefetch_count")]
    public int PrefetchCount { get; set; }

    [JsonPropertyName("channel_details")]
    public RabbitMqConsumerChannelDetails ChannelDetails { get; set; } = new();

    [JsonPropertyName("queue")]
    public RabbitMqConsumerQueueModel Queue { get; set; } = new();
}