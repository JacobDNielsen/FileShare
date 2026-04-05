namespace Storage.Configuration;

public class OpenFgaOptions
{
    public const string SectionName = "OpenFga";

    public string BaseUrl { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string AuthorizationModelId { get; set; } = string.Empty;
}