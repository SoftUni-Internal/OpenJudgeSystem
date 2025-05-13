using IntegrationTests.Worker;

[CollectionDefinition("Worker Tests Collection", DisableParallelization = true)]
public class WorkerTestsCollection : ICollectionFixture<RabbitMqAndWorkerFixture>
{
}