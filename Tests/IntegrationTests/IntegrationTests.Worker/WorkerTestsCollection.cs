using IntegrationTests.Worker;

[CollectionDefinition(nameof(WorkerTestsCollection), DisableParallelization = true)]
public class WorkerTestsCollection : ICollectionFixture<RabbitMqAndWorkerFixture>
{
}