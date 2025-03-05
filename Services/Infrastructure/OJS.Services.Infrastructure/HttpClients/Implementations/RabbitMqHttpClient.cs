namespace OJS.Services.Infrastructure.HttpClients.Implementations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Services.Infrastructure.Configurations;
using OJS.Services.Infrastructure.HttpClients.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class RabbitMqHttpClient : HttpClientService, IRabbitMqHttpClient
{
    private readonly MessageQueueConfig mqConfig;

    public RabbitMqHttpClient(
        HttpClient client,
        ILogger<RabbitMqHttpClient> logger,
        IOptions<MessageQueueConfig> messageQueueConfigAccessor,
        IOptions<ApplicationUrlsConfig> applicationUrlsConfigAccessor)
        : base(client, logger, string.Empty)
    {
        this.mqConfig = messageQueueConfigAccessor.Value;
        var authString = $"{this.mqConfig.User}:{this.mqConfig.Password}";
        var base64Auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authString));

        var hostValue = this.mqConfig.Host;
        if (!hostValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !hostValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            hostValue = "http://" + hostValue;
        }

        client.BaseAddress = new Uri($"{hostValue}:{applicationUrlsConfigAccessor.Value.RabbitMqHttpApiPort}");
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
    }

    public async Task<IEnumerable<RabbitMqConsumerModel>> GetConsumers()
    {
        var consumers = await this.Get<IEnumerable<RabbitMqConsumerModel>>($"/api/consumers/{this.mqConfig.VirtualHost}");
        return consumers ?? [];
    }

    public async Task<IEnumerable<RabbitMqChannelModel>> GetChannels()
    {
        var channels = await this.Get<IEnumerable<RabbitMqChannelModel>>($"/api/vhosts/{this.mqConfig.VirtualHost}/channels");
        return channels ?? [];
    }

    public async Task<RabbitMqQueueModel?> GetQueue(string queueName)
        => await this.Get<RabbitMqQueueModel>($"api/queues/{this.mqConfig.VirtualHost}/{queueName}");
}