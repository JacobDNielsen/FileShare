public sealed class StorageClient : IStorageClient
{
    private readonly HttpClient _http;
    public StorageClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<object>> GetAllMetadataAsync(CancellationToken ct)
        => (await _http.GetFromJsonAsync<List<object>>("/wopi/files", ct))!; 

    public Task<CheckFileInfoResponse?> CheckFileInfoAsync(string fileId, CancellationToken ct)
        => _http.GetFromJsonAsync<CheckFileInfoResponse>($"/wopi/files/{Uri.EscapeDataString(fileId)}", ct);

    public async Task<Stream> GetContentsAsync(string fileId, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"/wopi/files/{Uri.EscapeDataString(fileId)}/contents");
        var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync(ct);
    }

    public async Task<HttpResponseMessage> UploadAsync(Stream content, string fileName, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StreamContent(content), "file", fileName); // matches [FromForm] FileUploadReq.File
        var resp = await _http.PostAsync("/wopi/files/upload", form, ct);
        return resp; // caller can read created metadata/location if needed
    }

    public async Task<bool> RenameAsync(string fileId, string newName, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(
            $"/wopi/files/{Uri.EscapeDataString(fileId)}/rename",
            new { baseFileName = newName }, ct);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string fileId, CancellationToken ct)
        => (await _http.DeleteAsync($"/wopi/files/{Uri.EscapeDataString(fileId)}", ct)).IsSuccessStatusCode;
}