using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Storage.Configuration;
using Storage.Interfaces;

namespace Storage.Services;

public class OpenFgaTupleWriter : IOpenFgaTupleWriter
{
    private readonly HttpClient _httpClient;
    private readonly OpenFgaOptions _options;
    private readonly ILogger<OpenFgaTupleWriter> _logger;

    public OpenFgaTupleWriter(HttpClient httpClient, IOptions<OpenFgaOptions> options, ILogger<OpenFgaTupleWriter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
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

        _logger.LogDebug("OpenFGA tuple write: user=user:{UserId}, object=file:{FileId}, status={Status}",
            userId, fileId, (int)response.StatusCode);

        response.EnsureSuccessStatusCode();
    }

    private sealed record WriteTuplesRequest
    {
        [JsonPropertyName("writes")]
        public required WritesPayload Writes { get; init; }

        [JsonPropertyName("authorization_model_id")]
        public required string AuthorizationModelId { get; init; }
    }

    private sealed record WritesPayload
    {
        [JsonPropertyName("tuple_keys")]
        public required List<TupleKey> TupleKeys { get; init; }
    }

    private sealed record TupleKey
    {
        [JsonPropertyName("user")]
        public required string User { get; init; }

        [JsonPropertyName("relation")]
        public required string Relation { get; init; }

        [JsonPropertyName("object")]
        public required string Object { get; init; }
    }
}