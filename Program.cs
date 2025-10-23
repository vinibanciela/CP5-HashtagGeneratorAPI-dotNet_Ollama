using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Globalization;
using HashtagGenerator.Api.Models;
using HashtagGenerator.Api.Ollama;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ollama", c =>
{
    c.BaseAddress = new Uri("http://localhost:11434");
    c.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Health
app.MapGet("/", () => Results.Ok(new { ok = true, service = "Hashtag Generator API" }));

// POST /hashtags
app.MapPost("/hashtags", async (HashtagRequest body, IHttpClientFactory httpFactory) =>
{
    // 1) Validação de entrada
    var text = body.Text?.Trim();
    if (string.IsNullOrWhiteSpace(text))
        return Results.BadRequest(new ErrorResponse("Campo 'text' é obrigatório e não pode ser vazio."));

    // modelo padrão: phi4-mini (pode sobrescrever no body)
    var model = string.IsNullOrWhiteSpace(body.Model) ? "llama3.2:3b" : body.Model!.Trim();

    int count = body.Count ?? 10; // padrão = 10
    if (count <= 0) return Results.BadRequest(new ErrorResponse("'count' deve ser > 0."));
    if (count > 30) return Results.BadRequest(new ErrorResponse("'count' deve ser <= 30."));

    // 2) Prompts
    var promptStructured = $"""
    Você é um gerador de hashtags. Gere exatamente {count} hashtags curtas, relevantes e populares para o texto a seguir.

    Regras obrigatórias:
    - Retorne APENAS JSON que siga exatamente o schema informado (sem comentários, sem texto extra).
    - As hashtags devem começar com '#' e não conter espaços (CamelCase/underscore se necessário).
    - Não repita hashtags.
    - Não inclua explicações.

    Texto de entrada (contexto):
    {text}

    Agora produza o JSON final.
    """;

    var promptPlain = $$"""
    Gere exatamente {{count}} hashtags curtas, relevantes e populares.

    Regras:
    - Comecem com '#'
    - Sem espaços (use CamelCase ou underscore)
    - Sem duplicatas
    - Não inclua nenhuma explicação

    Retorne EXCLUSIVAMENTE o JSON a seguir (sem texto extra, sem linhas antes ou depois):
    {
    "hashtags": ["#tag1", "#tag2"]
    }

    Texto de entrada:
    {{text}}
    """;

    // 3) JSON Schema para structured outputs
    var schema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["hashtags"] = new JsonObject
            {
                ["type"] = "array",
                ["minItems"] = count,
                ["maxItems"] = count,
                ["items"] = new JsonObject { ["type"] = "string" }
            }
        },
        ["required"] = new JsonArray("hashtags"),
        ["additionalProperties"] = false
    };

    var reqStructured = new OllamaRequest
    {
        Model = model,
        Prompt = promptStructured,
        Stream = false,
        // structured outputs
        Format = new JsonObject
        {
            ["type"] = "json_schema",
            ["json_schema"] = new JsonObject
            {
                ["name"] = "HashtagList",
                ["schema"] = schema
            }
        }
    };

    var client = httpFactory.CreateClient("ollama");

    // 4) Tentativa 1: json_schema
    HttpResponseMessage resp;
    try
    {
        resp = await client.PostAsJsonAsync("/api/generate", reqStructured);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorResponse($"Erro ao contatar Ollama: {ex.Message}"));
    }

    // 5) Fallback: alguns modelos/versões retornam 500 "invalid JSON schema"
    if (!resp.IsSuccessStatusCode)
    {
        var errText = await resp.Content.ReadAsStringAsync();
        var isSchemaError = (int)resp.StatusCode == 500 &&
                            errText.Contains("invalid JSON schema", StringComparison.OrdinalIgnoreCase);

        if (!isSchemaError)
            return Results.BadRequest(new ErrorResponse($"Ollama respondeu {(int)resp.StatusCode} {resp.ReasonPhrase}: {errText}"));

        var reqPlain = new OllamaRequest
        {
            Model = model,
            Prompt = promptPlain,
            Stream = false,
            Format = "json" // <- chave do fallback
        };

        try
        {
            resp = await client.PostAsJsonAsync("/api/generate", reqPlain);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new ErrorResponse($"Erro ao contatar Ollama (fallback): {ex.Message}"));
        }

        if (!resp.IsSuccessStatusCode)
        {
            var err2 = await resp.Content.ReadAsStringAsync();
            return Results.BadRequest(new ErrorResponse($"Ollama (fallback) respondeu {(int)resp.StatusCode} {resp.ReasonPhrase}: {err2}"));
        }
    }

    // 6) Ler resposta do Ollama (serve para as duas tentativas)
    OllamaResponse? ollama;
    try
    {
        ollama = await resp.Content.ReadFromJsonAsync<OllamaResponse>();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorResponse($"Falha ao desserializar resposta do Ollama: {ex.Message}"));
    }

    if (ollama is null || string.IsNullOrWhiteSpace(ollama.Response))
        return Results.BadRequest(new ErrorResponse("Resposta do Ollama não contém dados."));

    JsonObject? generated;
    try
    {
        generated = JsonNode.Parse(ollama.Response)?.AsObject();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new ErrorResponse($"Conteúdo gerado não é um JSON válido: {ex.Message}"));
    }

    if (generated is null || !generated.TryGetPropertyValue("hashtags", out var hashtagsNode) || hashtagsNode is not JsonArray)
        return Results.BadRequest(new ErrorResponse("JSON gerado não segue o schema esperado."));

    var rawList = hashtagsNode!.AsArray().Select(n => n?.GetValue<string>() ?? string.Empty).ToList();

    // 7) Sanitização: #, sem espaços, sem duplicatas
    static string Normalize(string s)
    {
        s = s.Trim();
        if (!s.StartsWith('#')) s = "#" + s;
        s = new string(s.Where(ch => ch != ' ').ToArray());
        return s;
    }

    var cleaned = rawList
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(Normalize)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    // modo estrito: exigir N distintos
    if (cleaned.Count < count)
    {
        return Results.BadRequest(new ErrorResponse(
            $"O modelo não conseguiu gerar as {count} hashtags únicas solicitadas (gerou {cleaned.Count}). " +
            "Tente um texto mais descritivo ou um modelo diferente."
        ));
    }

    // se por algum motivo vieram mais que N, restringe
    var finalHashtags = cleaned.Take(count).ToList();

    var result = new HashtagResponse
    {
        Model = model,
        Count = count,              // <- mantém o N solicitado
        Hashtags = finalHashtags
    };

    return Results.Ok(result);
}).WithName("GenerateHashtags");

app.Run();
