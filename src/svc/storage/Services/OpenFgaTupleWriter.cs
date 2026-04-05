using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Storage.Configuration;
using Storage.Interfaces;

namespace Storage.Services;

public class OpenFgaTupleWriter : IOpenFgaTupleWriter
{
    private readonly HttpClient _httpClient;
    private readonly OpenFgaOptions _options;

    public OpenFgaTupleWriter(HttpClient httpClient, IOptions<OpenFgaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task WriteOwnerTupleAsync(string userId, string fileId, CancellationToken cancellationToken = default)
    {
        var request = new WriteTuplesRequest
        {
            Writes = new WritesPayload
            {
                TupleKeys =
                [
                    new TupleKey
                    {
                        User = $"user:{userId}",
                        Relation = "owner",
                        Object = $"file:{fileId}"
                    }
                ]
            },
            AuthorizationModelId = _options.AuthorizationModelId
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"/stores/{_options.StoreId}/write",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        Console.WriteLine("OpenFGA tuple write:");
        Console.WriteLine($"StoreId: {_options.StoreId}");
        Console.WriteLine($"ModelId: {_options.AuthorizationModelId}");
        Console.WriteLine($"User: user:{userId}");
        Console.WriteLine($"Relation: owner");
        Console.WriteLine($"Object: file:{fileId}");
        Console.WriteLine($"Status: {(int)response.StatusCode}");
        Console.WriteLine($"Response: {responseBody}");

        response.EnsureSuccessStatusCode();
    }

    private sealed class WriteTuplesRequest
    {
        [JsonPropertyName("writes")]
        public WritesPayload Writes { get; set; } = new();

        [JsonPropertyName("authorization_model_id")]
        public string AuthorizationModelId { get; set; } = string.Empty;
    }

    private sealed class WritesPayload
    {
        [JsonPropertyName("tuple_keys")]
        public List<TupleKey> TupleKeys { get; set; } = [];
    }

    private sealed class TupleKey
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("relation")]
        public string Relation { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    }
}