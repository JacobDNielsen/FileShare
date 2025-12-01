using System.Reflection.Metadata.Ecma335;

public class LockClient : ILockClient
{

    private readonly HttpClient _http;

    public LockClient(HttpClient http) => _http = http;
    public async Task<GetLockResponse?> GetLockAsync(string fileId, CancellationToken ct)
    {
        
        var result = await _http.GetAsync($"/locks/{Uri.EscapeDataString(fileId)}", ct);
        if(!result.IsSuccessStatusCode) 
            return null;
        return await result.Content.ReadFromJsonAsync<GetLockResponse>(cancellationToken: ct);
    }

    public async Task<LockResponse> SetLockAsync(string fileId, LockRequest dto, CancellationToken ct)
    {
        var result = await _http.PostAsJsonAsync($"/locks/{Uri.EscapeDataString(fileId)}", dto, ct);
        if(!result.IsSuccessStatusCode)
            return new LockResponse{Success = false};
        return await result.Content.ReadFromJsonAsync<LockResponse>(cancellationToken: ct)
            ?? new LockResponse{Success = false};  
    }

    public async Task<LockResponse> UnlockAsync(string fileId, LockRequest dto, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/locks/{Uri.EscapeDataString(fileId)}")
        {
          Content = JsonContent.Create(dto)  
        };
        var result = await _http.SendAsync(request, ct);
        if(!result.IsSuccessStatusCode)
            return new LockResponse{Success = false};
        return await result.Content.ReadFromJsonAsync<LockResponse>(cancellationToken: ct)
            ?? new LockResponse{Success = false};

    }

    public async Task<LockResponse> RefreshLockAsync(string fileId, LockRequest dto, CancellationToken ct)
    {
        var result = await _http.PostAsJsonAsync($"/locks/{Uri.EscapeDataString(fileId)}/refresh", dto, ct);
        if(!result.IsSuccessStatusCode)
            return new LockResponse{Success = false};
        return await result.Content.ReadFromJsonAsync<LockResponse>(cancellationToken: ct)
            ?? new LockResponse{Success = false};
    }
}