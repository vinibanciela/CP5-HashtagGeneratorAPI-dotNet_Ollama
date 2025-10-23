using System.Text.Json.Serialization;

namespace HashtagGenerator.Api.Models;

public sealed class HashtagRequest
{
    [JsonPropertyName("text")] public string? Text { get; set; }
    [JsonPropertyName("count")] public int? Count { get; set; } // padrão = 10, máx. 30
    [JsonPropertyName("model")] public string? Model { get; set; } // padrão = llama3.2:3b
}
