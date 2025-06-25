namespace IntegrationTests.Worker;

public class MassTransitEnvelope
{
    // Used for deserialization
    public MassTransitEnvelope()
    {
    }

    public MassTransitEnvelope(
        int rabbitMqPort,
        string endpointFullyQualifiedName,
        string modelNamespace,
        string modelName,
        object message)
    {
        this.SourceAddress = $"rabbitmq://localhost:{rabbitMqPort}/test-source";
        this.DestinationAddress = $"rabbitmq://localhost:{rabbitMqPort}/ojs/{endpointFullyQualifiedName}";
        this.MessageType = [$"urn:message:{modelNamespace}:{modelName}"];
        this.Message = message;
    }

    /// <summary>
    /// The envelope's id.
    /// </summary>
    public Guid MessageId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The source from which the message is sent. Using 'test-source' by default.
    /// </summary>
    public string SourceAddress { get; set; }

    /// <summary>
    /// The destination to which the message is sent.
    /// -> endpointFullyQualifiedName is the endpoint's fully qualified name, as this is the way we register Mass Transit's endpoints.
    /// </summary>
    public string DestinationAddress { get; set; }

    /// <summary>
    /// The type of the message to be sent.
    /// -> modelNamespace is the model's namespace.
    /// -> modelName is the model's class name.
    /// </summary>
    public string[] MessageType { get; set; }

    /// <summary>
    /// The message to be sent.
    /// </summary>
    public object Message { get; set; }
}