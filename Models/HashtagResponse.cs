using System.Text.Json.Serialization;

namespace HashtagGenerator.Api.Models;

public sealed class HashtagResponse
{
    [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("hashtags")] public List<string> Hashtags { get; set; } = new();
}
