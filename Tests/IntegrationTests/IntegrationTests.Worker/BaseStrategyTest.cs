namespace IntegrationTests.Worker;

public abstract class BaseStrategyTest<TFactory, TParameters>
    where TFactory : BaseSubmissionFactory<TParameters>
{
    protected BaseStrategyTest(RabbitMqAndWorkerFixture fixture, TFactory factory)
    {
        this.Fixture = fixture;
        this.Factory = factory;
    }

    protected TFactory Factory { get; set; }

    protected RabbitMqAndWorkerFixture Fixture { get; set; }
}