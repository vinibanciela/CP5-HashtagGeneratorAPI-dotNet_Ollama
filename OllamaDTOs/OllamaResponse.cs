using System.Text.Json.Serialization;

namespace HashtagGenerator.Api.Ollama;

public sealed class OllamaResponse
{
    [JsonPropertyName("model")] public string? Model { get; set; }
    [JsonPropertyName("created_at")] public string? CreatedAt { get; set; }
    // Quando format=json/json_schema, o conteúdo vem como string JSON em 'response'
    [JsonPropertyName("response")] public string? Response { get; set; }
    [JsonPropertyName("done")] public bool Done { get; set; }
}
