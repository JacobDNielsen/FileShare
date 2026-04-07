using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WopiHost.Configuration;
using System.Text.Json.Serialization;



public class OpenFgaService : IOpenFgaService
{
    private readonly HttpClient _httpClient;
    private readonly OpenFgaOptions _options;

    public OpenFgaService(
        HttpClient httpClient,
        IOptions<OpenFgaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
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

            if (isDevelopment){
            Console.WriteLine("OpenFGA request:");
            Console.WriteLine($"StoreId: {_options.StoreId}");
            Console.WriteLine($"ModelId: {_options.AuthorizationModelId}");
            Console.WriteLine($"User: {user}");
            Console.WriteLine($"Relation: {relation}");
            Console.WriteLine($"Object: {obj}");
            Console.WriteLine($"Status: {(int)response.StatusCode}");
            Console.WriteLine($"Response: {responseBody}");
            }

            response.EnsureSuccessStatusCode();

            var result = System.Text.Json.JsonSerializer.Deserialize<OpenFgaCheckResponse>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result?.Allowed ?? false;
}

    private sealed class OpenFgaCheckRequest
    {
        [JsonPropertyName("tuple_key")]
        public OpenFgaTupleKey TupleKey { get; set; } = new();

        [JsonPropertyName("authorization_model_id")]
        public string AuthorizationModelId { get; set; } = string.Empty;
    }

    private sealed class OpenFgaTupleKey
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("relation")]
        public string Relation { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    }

    private sealed class OpenFgaCheckResponse
    {
        [JsonPropertyName("allowed")]
        public bool Allowed { get; set; }
    }
}
