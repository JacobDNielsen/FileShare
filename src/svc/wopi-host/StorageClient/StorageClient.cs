using WopiHost.Dto;

public sealed class StorageClient : IStorageClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public StorageClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("Authorization", authHeader);
        }

        var response =  await _http.GetAsync($"/wopi/files/{Uri.EscapeDataString(fileId)}", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CheckFileInfoResponse>(cancellationToken: ct);
    }
    public async Task<Stream> GetFile(string fileId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/wopi/files/{Uri.EscapeDataString(fileId)}/contents");
        var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync(ct);
    }
}