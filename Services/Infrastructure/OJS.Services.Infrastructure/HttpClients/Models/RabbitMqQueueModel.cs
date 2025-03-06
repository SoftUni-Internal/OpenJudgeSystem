namespace OJS.Services.Infrastructure.HttpClients.Models;

using System.Text.Json.Serialization;

public class RabbitMqQueueModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The number messages in the queue that have been delivered to clients but have not yet been acknowledged (in flight).
    /// </summary>
    [JsonPropertyName("messages_unacknowledged")]
    public int MessagesUnacknowledged { get; set; }

    /// <summary>
    /// The number of messages in the queue that are ready to be delivered to clients. (not yet delivered to a consumer)
    /// </summary>
    [JsonPropertyName("messages_ready")]
    public int MessagesReady { get; set; }

    /// <summary>
    /// Total number of messages in the queue (messages_unacknowledged + messages_ready).
    /// </summary>
    [JsonPropertyName("messages")]
    public int Messages { get; set; }

    [JsonPropertyName("vhost")]
    public string? VHost { get; set; }
}