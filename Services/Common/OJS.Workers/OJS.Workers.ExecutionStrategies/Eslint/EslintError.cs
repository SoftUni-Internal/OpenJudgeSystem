namespace OJS.Workers.ExecutionStrategies.Eslint;

using System.Text.Json.Serialization;

public class EslintError
{
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<EslintMessage> Messages { get; set; } = [];
}