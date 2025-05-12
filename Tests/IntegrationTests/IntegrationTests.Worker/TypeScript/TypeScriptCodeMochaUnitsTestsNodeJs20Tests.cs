namespace IntegrationTests.Worker.TypeScript;

public class TypeScriptCodeMochaUnitsTestsNodeJs20Tests : BaseStrategyTest<TypeScriptCodeMochaUnitsTestsNodeJs20SubmissionFactory, TypeScriptCodeMochaUnitsTestsNodeJs20Parameters>, IClassFixture<RabbitMqAndWorkerFixture>
{
    public TypeScriptCodeMochaUnitsTestsNodeJs20Tests(RabbitMqAndWorkerFixture fixture, TypeScriptCodeMochaUnitsTestsNodeJs20SubmissionFactory factory)
        : base(fixture, factory) { }


}