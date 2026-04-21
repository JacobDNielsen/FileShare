using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WopiHost.Configuration;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;



public class OpenFgaService : IOpenFgaService
{
    private readonly HttpClient _httpClient;
    private readonly OpenFgaOptions _options;
    private readonly ILogger<OpenFgaService> _logger;

    public OpenFgaService(
        HttpClient httpClient,
        IOptions<OpenFgaOptions> options,
        ILogger<OpenFgaService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<bool> CanViewFileAsync(string userId, string fileId, CancellationToken cancellationToken = default)
        => CheckAsync($"user:{userId}", "can_view", $"file:{fileId}", cancellationToken);

    public Task<bool> CanEditFileAsync(string userId, string fileId, CancellationToken cancellationToken = default)
        => CheckAsync($"user:{userId}", "can_edit", $"file:{fileId}", cancellationToken);

    private async Task<bool> CheckAsync(
        string user,
        string relation,
        string obj,
        CancellationToken cancellationToken)
    {
        var request = new OpenFgaCheckRequest
        {
            TupleKey = new OpenFgaTupleKey
            {
                User = user,
                Relation = relation,
                Object = obj
            },
            AuthorizationModelId = _options.AuthorizationModelId
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"/stores/{_options.StoreId}/check",
            request,
            cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);


            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OpenFgaCheckResponse>(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        return result?.Allowed ?? false;
}

    private sealed record OpenFgaCheckRequest
    {
        [JsonPropertyName("tuple_key")]
        public required OpenFgaTupleKey TupleKey { get; init; }

        [JsonPropertyName("authorization_model_id")]
        public required string AuthorizationModelId { get; init; }
    }

    private sealed record OpenFgaTupleKey
    {
        [JsonPropertyName("user")]
        public required string User { get; init; }

        [JsonPropertyName("relation")]
        public required string Relation { get; init; }

        [JsonPropertyName("object")]
        public required string Object { get; init; }
    }

    private sealed record OpenFgaCheckResponse
    {
        [JsonPropertyName("allowed")]
        public bool Allowed { get; init; }
    }
}
