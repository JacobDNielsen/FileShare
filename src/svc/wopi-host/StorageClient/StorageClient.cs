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

    public async Task PutFileAsync(string fileId, Stream content, string fileName, long size, CancellationToken ct)
    {
        var req = new HttpRequestMessage(
            HttpMethod.Put,
            $"/wopi/files/{Uri.EscapeDataString(fileId)}/contents");

        // Stream the bytes
        req.Content = new StreamContent(content);
        req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        req.Content.Headers.ContentLength = size;

        // Pass metadata needed by FileService.OverwriteAsync
        // (Adapt header names to whatever your Storage API expects)
        req.Headers.Add("X-File-Name", fileName);
        req.Headers.Add("X-File-Size", size.ToString(System.Globalization.CultureInfo.InvariantCulture));

        var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        // We don't care about the body here; Storage service will update metadata + bytes
    }

}