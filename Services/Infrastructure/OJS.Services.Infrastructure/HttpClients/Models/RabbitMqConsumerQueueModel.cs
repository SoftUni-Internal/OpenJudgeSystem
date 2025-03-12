namespace OJS.Services.Infrastructure.HttpClients.Models;

using System.Text.Json.Serialization;

public class RabbitMqConsumerQueueModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("vhost")]
    public string? VHost { get; set; }
}