namespace IntegrationTests.Worker;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Servers.Ui.Consumers;
using OJS.Servers.Worker.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Testcontainers.RabbitMq;

public class RabbitMqAndWorkerFixture : IAsyncLifetime
{
    private readonly string rabbitMqUser = "ojsuser";
    private readonly string rabbitMqPassword = "myS3cretPass2";
    private readonly string rabbitMqVirtualHost = "ojs";
    private const string WorkerImageName = "judge_worker_intergration_tests:latest";
    private const string WorkerName = "worker_intergration_tests";

    private readonly RabbitMqContainer rabbitMqContainer;
    private readonly string projectRoot;
    private readonly Type submissionForProcessingType;
    private readonly Type processedSubmissionType;
    private readonly Type publishEndpointType;
    private readonly Type consumeEndpointType;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<ProcessedSubmissionPubSubModel?>> pendingSubmissions = new();
    private readonly object consumerLock = new();

    private IContainer workerContainer;
    private IConnection rabbitMqConnection;
    private IChannel publisherChannel;
    private IChannel consumerChannel;
    private int rabbitMqPort;
    private bool consumerInitialized;

    public RabbitMqAndWorkerFixture()
    {
        this.submissionForProcessingType = typeof(SubmissionForProcessingPubSubModel);
        this.publishEndpointType = typeof(SubmissionsForProcessingConsumer);
        this.processedSubmissionType = typeof(ProcessedSubmissionPubSubModel);
        this.consumeEndpointType = typeof(ExecutionResultConsumer);
        this.projectRoot = Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, ..Enumerable.Repeat("..", 6)]));

        var configPath = Path.Combine(this.projectRoot, "rabbitmq", "rabbitmq.conf");

        this.rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", this.rabbitMqUser)
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", this.rabbitMqPassword)
            .WithEnvironment("DEFAULT_USER_PASSWORD", this.rabbitMqPassword)
            .WithBindMount(configPath, "/etc/rabbitmq/rabbitmq.conf")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await this.rabbitMqContainer.StartAsync();
        this.rabbitMqPort = this.rabbitMqContainer.GetMappedPublicPort(5672);

        var executionStrategiesPath = Path.Combine(Path.GetTempPath(), "ExecutionStrategies");

        if (!Directory.Exists(executionStrategiesPath))
        {
            Directory.CreateDirectory(executionStrategiesPath);
        }

        var dockerCompatiblePath = executionStrategiesPath.Replace("\\", "/");

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole();
        });

        var workerImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(this.projectRoot)
            .WithDockerfile("./Docker/Dockerfile.worker")
            .WithName(WorkerImageName)
            .WithLogger(loggerFactory.CreateLogger<ImageFromDockerfileBuilder>())
            .Build();

        await workerImage.CreateAsync();

        this.workerContainer = new ContainerBuilder()
            .WithImage(WorkerImageName)
            .WithName(WorkerName)
            .WithHostname(WorkerName)
            .WithEnvironment("MessageQueue__Host", this.rabbitMqContainer.IpAddress)
            .WithEnvironment("DOTNET_ENVIRONMENT", "Production")
            .WithBindMount(dockerCompatiblePath, "/app/ExecutionStrategies")
            .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock")
            .DependsOn(this.rabbitMqContainer)
            .WithLogger(loggerFactory.CreateLogger<ImageFromDockerfileBuilder>())
            .Build();

        await this.workerContainer.StartAsync();

        var connectionFactory = new ConnectionFactory
        {
            HostName = "localhost",
            VirtualHost = this.rabbitMqVirtualHost,
            UserName = this.rabbitMqUser,
            Password = this.rabbitMqPassword,
            Port = this.rabbitMqPort,
        };

        this.rabbitMqConnection = await connectionFactory.CreateConnectionAsync();
        this.publisherChannel = await this.rabbitMqConnection.CreateChannelAsync();
        this.consumerChannel = await this.rabbitMqConnection.CreateChannelAsync();

        await using var setupChannel = await this.rabbitMqConnection.CreateChannelAsync();

        var inputQueueName = this.publishEndpointType.FullName;
        var outputQueueName = this.consumeEndpointType.FullName;
        var outputExchangeName = $"{this.processedSubmissionType.Namespace}:{this.processedSubmissionType.Name}";

        await setupChannel.ExchangeDeclareAsync(exchange: outputExchangeName, type: "fanout", durable: true, autoDelete: false);
        await setupChannel.QueueDeclareAsync(queue: outputQueueName, durable: true, exclusive: false, autoDelete: false);
        await setupChannel.QueueBindAsync(queue: outputQueueName, exchange: outputExchangeName, routingKey: "");
        await setupChannel.QueueDeclareAsync(queue: inputQueueName, durable: true, exclusive: false, autoDelete: false);
    }

    public async Task<ProcessedSubmissionPubSubModel?> ProcessSubmission(SubmissionForProcessingPubSubModel submission)
    {
        await this.PublishMessage(submission);

        return await this.ConsumeMessage(CancellationToken.None, submission.Id);
    }

    private Task<ProcessedSubmissionPubSubModel?> ConsumeMessage(CancellationToken token, int submissionId)
    {
        var tcs = new TaskCompletionSource<ProcessedSubmissionPubSubModel?>();
        this.pendingSubmissions[submissionId] = tcs;

        if (!this.consumerInitialized)
        {
            lock (this.consumerLock)
            {
                if (!this.consumerInitialized)
                {
                    this.InitializeConsumer();
                    this.consumerInitialized = true;
                }
            }
        }

        return tcs.Task.WaitAsync(token);
    }

    private void InitializeConsumer()
    {
        var consumer = new AsyncEventingBasicConsumer(this.consumerChannel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var envelope = JsonSerializer.Deserialize<MassTransitEnvelope>(message, options);

                if (envelope == null)
                {
                    Console.WriteLine("Failed to deserialize MassTransit envelope.");
                    return;
                }

                Console.WriteLine($"Extracted raw message: {JsonSerializer.Serialize(envelope.Message)}");

                ProcessedSubmissionPubSubModel? receivedSubmission = null;
                if (envelope.Message is JsonElement jsonElement)
                {
                    receivedSubmission = jsonElement.Deserialize<ProcessedSubmissionPubSubModel>(options);
                }

                if (receivedSubmission != null)
                {
                    if (this.pendingSubmissions.TryRemove(receivedSubmission.Id, out var pendingTcs))
                    {
                        pendingTcs.SetResult(receivedSubmission);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing received message: {ex.Message}");
            }

            await Task.CompletedTask;
        };

        this.consumerChannel.BasicConsumeAsync(
            queue: this.consumeEndpointType.FullName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: CancellationToken.None).GetAwaiter().GetResult();
    }

    private async Task PublishMessage(SubmissionForProcessingPubSubModel submission)
    {
        var envelope = new MassTransitEnvelope(
            this.rabbitMqPort,
            this.publishEndpointType.FullName!,
            this.submissionForProcessingType.Namespace!,
            this.submissionForProcessingType.Name,
            submission);
        var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope)).AsMemory();
        var publishQueueName = this.publishEndpointType.FullName;

        var basicProperties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/vnd.masstransit+json" }
            }!
        };

        await this.publisherChannel.BasicPublishAsync(
            exchange: "",
            routingKey: publishQueueName,
            mandatory: false,
            basicProperties: basicProperties,
            body: messageBody,
            cancellationToken: CancellationToken.None
        );
    }

    public async Task DisposeAsync()
    {
        try
        {
            await this.publisherChannel.CloseAsync();
            this.publisherChannel.Dispose();

            await this.consumerChannel.CloseAsync();
            this.consumerChannel.Dispose();

            await this.rabbitMqConnection.CloseAsync();
            this.rabbitMqConnection.Dispose();

            await this.workerContainer.StopAsync();
            await this.workerContainer.DisposeAsync();

            await this.rabbitMqContainer.StopAsync();
            await this.rabbitMqContainer.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
    }
}