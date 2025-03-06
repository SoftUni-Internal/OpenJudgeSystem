namespace OJS.Services.Infrastructure.HttpClients.Models;

using System.Text.Json.Serialization;

public class RabbitMqChannelModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Number of messages that are currently being processed by consumers.
    /// </summary>
    [JsonPropertyName("messages_unacknowledged")]
    public int MessagesUnacknowledged { get; set; }

    /// <summary>
    /// How many message is the channel allowed to prefetch.
    /// </summary>
    [JsonPropertyName("prefetch_count")]
    public int PrefetchCount { get; set; }

    /// <summary>
    /// Number of consumers that are currently connected to the channel.
    /// </summary>
    [JsonPropertyName("consumer_count")]
    public int ConsumerCount { get; set; }
}