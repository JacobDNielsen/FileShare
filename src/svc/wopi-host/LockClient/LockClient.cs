public class LockClient : ILockClient
{
    private readonly HttpClient _http;

    public LockClient(HttpClient http) => _http = http;

    public Task<HttpResponseMessage> GetLockAsync(string fileId, CancellationToken ct)
        => _http.GetAsync($"wopi/locks/{Uri.EscapeDataString(fileId)}", ct);

    public Task<HttpResponseMessage> LockAsync(string fileId, string lockValue, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"wopi/locks/{Uri.EscapeDataString(fileId)}");
        request.Headers.Add("X-WOPI-Lock", lockValue);
        return _http.SendAsync(request, ct);
    }

    public Task<HttpResponseMessage> RefreshLockAsync(string fileId, string lockValue, CancellationToken ct)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"wopi/locks/{Uri.EscapeDataString(fileId)}/refresh");

        request.Headers.Add("X-WOPI-Lock", lockValue);
        return _http.SendAsync(request, ct);
    }

    public Task<HttpResponseMessage> UnlockAsync(string fileId, string lockValue, CancellationToken ct)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"wopi/locks/{Uri.EscapeDataString(fileId)}");

        request.Headers.Add("X-WOPI-Lock", lockValue);
        return _http.SendAsync(request, ct);
    }
}