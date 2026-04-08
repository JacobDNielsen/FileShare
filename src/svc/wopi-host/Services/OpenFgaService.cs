using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Claims;


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

     public Task<bool> CanReadFileAsync(string userId, string fileId, CancellationToken ct = default) =>
        CheckAsync(
            user: ToFgaUser(userId),
            relation: "can_read",
            obj: ToFgaFile(fileId),
            ct);

    public Task<bool> CanWriteFileAsync(string userId, string fileId, CancellationToken ct = default) =>
        CheckAsync(
            user: ToFgaUser(userId),
            relation: "can_write",
            obj: ToFgaFile(fileId),
            ct);

    public Task<bool> CanDeleteFileAsync(string userId, string fileId, CancellationToken ct = default) =>
        CheckAsync(
            user: ToFgaUser(userId),
            relation: "can_delete",
            obj: ToFgaFile(fileId),
            ct);

    public Task<bool> CanUploadToPatientAsync(string userId, string patientId, CancellationToken ct = default) =>
        CheckAsync(
            user: ToFgaUser(userId),
            relation: "can_upload_files",
            obj: ToFgaPatient(patientId),
            ct);

    private async Task<bool> CheckAsync(string user, string relation, string obj, CancellationToken ct)
    {
        var request = new CheckRequest
        {
            AuthorizationModelId = _options.AuthorizationModelId,
            TupleKey = new TupleKey
            {
                User = user,
                Relation = relation,
                Object = obj
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/stores/{_options.StoreId}/check",
            request,
            ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenFGA check failed. Status={StatusCode}, User={User}, Relation={Relation}, Object={Object}, Body={Body}",
                (int)response.StatusCode, user, relation, obj, body);

            return false;
        }

        var result = System.Text.Json.JsonSerializer.Deserialize<CheckResponse>(body);

        _logger.LogInformation(
            "OpenFGA check result. User={User}, Relation={Relation}, Object={Object}, Allowed={Allowed}",
            user, relation, obj, result?.Allowed);

        return result?.Allowed ?? false;
    }

    private static string ToFgaUser(string userId) => $"user:{userId}";
    private static string ToFgaFile(string fileId) => $"file:{fileId}";
    private static string ToFgaPatient(string patientId) => $"patient:{patientId}";
}
