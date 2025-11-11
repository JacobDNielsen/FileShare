using WopiHost.Dto;
public sealed class StorageClient : IStorageClient
{
    private readonly HttpClient _http;
    public StorageClient(HttpClient http) => _http = http;

    public Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct)
        => _http.GetFromJsonAsync<CheckFileInfoResponse>($"/wopi/files/{Uri.EscapeDataString(fileId)}", ct);

    public async Task<Stream> GetFile(string fileId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/wopi/files/{Uri.EscapeDataString(fileId)}/contents");
        var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync(ct);
    }
}