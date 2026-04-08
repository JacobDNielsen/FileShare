public sealed class OpenFgaOptions
{
    public const string SectionName = "OpenFga";

    public string BaseUrl { get; set; } = default!;
    public string StoreId { get; set; } = default!;
    public string AuthorizationModelId { get; set; } = default!;
}