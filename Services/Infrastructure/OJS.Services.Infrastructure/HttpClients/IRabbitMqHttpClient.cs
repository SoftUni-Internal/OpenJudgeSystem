namespace OJS.Services.Infrastructure.HttpClients;

using OJS.Services.Infrastructure.HttpClients.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IRabbitMqHttpClient : IHttpClientService
{
    Task<IEnumerable<RabbitMqConsumerModel>> GetConsumers();


    Task<IEnumerable<RabbitMqChannelModel>> GetChannels();

    Task<RabbitMqQueueModel?> GetQueue(string queueName);
}