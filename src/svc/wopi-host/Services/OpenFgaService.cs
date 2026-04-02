using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WopiHost.Configuration;



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

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenFgaCheckResponse>(cancellationToken: cancellationToken);

        return result?.Allowed ?? false;
    }

    private sealed class OpenFgaCheckRequest
    {
        public OpenFgaTupleKey TupleKey { get; set; } = new();
        public string AuthorizationModelId { get; set; } = string.Empty;
    }

    private sealed class OpenFgaTupleKey
    {
        public string User { get; set; } = string.Empty;
        public string Relation { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;
    }

    private sealed class OpenFgaCheckResponse
    {
        public bool Allowed { get; set; }
    }
}
