namespace OJS.Workers.ExecutionStrategies.Eslint;

using System.Text.Json.Serialization;

public class EslintMessage
{
    [JsonPropertyName("line")]
    public int Line { get; set; }

    [JsonPropertyName("column")]
    public int Column { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}