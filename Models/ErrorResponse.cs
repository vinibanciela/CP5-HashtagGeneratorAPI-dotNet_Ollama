using System.Text.Json.Serialization;

namespace HashtagGenerator.Api.Models;

public sealed class ErrorResponse
{
    public ErrorResponse(string message) => Message = message;
    [JsonPropertyName("message")] public string Message { get; }
}
