namespace OJS.Services.Infrastructure.HttpClients.Models;

using Newtonsoft.Json;

public class RabbitMqConsumerChannelDetails
{
    [JsonProperty("name")]
    public string? Name { get; set; }
}