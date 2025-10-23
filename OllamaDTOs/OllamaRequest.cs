using System.Text.Json.Serialization;

namespace HashtagGenerator.Api.Ollama;

public sealed class OllamaRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("prompt")] public string Prompt { get; set; } = string.Empty;
    [JsonPropertyName("stream")] public bool Stream { get; set; } = false;

    // Aceita tanto { type: "json_schema", ... } quanto "json"
    [JsonPropertyName("format")] public object? Format { get; set; }

    // Opcional: temperatura etc.
    // [JsonPropertyName("options")] public object? Options { get; set; }
}
