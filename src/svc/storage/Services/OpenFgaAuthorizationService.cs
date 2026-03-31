public class OpenFgaAuthorizationService : IAuthorizationService
{
    private readonly HttpClient _http;
    private readonly OpenFgaConfig _config;

    public OpenFgaAuthorizationService(HttpClient http, IOptions<OpenFgaConfig> config)
    {
        _http = http;
        _config = config.Value;
    }

    public async Task<bool> CanViewFile(string userId, string fileId)
    {
        var request = new
        {
            tuple_key = new
            {
                user = $"user:{userId}",
                relation = "can_view",
                @object = $"file:{fileId}"
            },
            authorization_model_id = _config.AuthorizationModelId
        };

        var url = $"{_config.ApiUrl}/stores/{_config.StoreId}/check";

        var response = await _http.PostAsJsonAsync(url, request);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<CheckResponse>();

        return result?.allowed ?? false;
    }

    private class CheckResponse
    {
        public bool allowed { get; set; }
    }
}