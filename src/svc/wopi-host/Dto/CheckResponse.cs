using System.Text.Json.Serialization;
public sealed class CheckResponse
{
    [JsonPropertyName("allowed")]
    public bool Allowed { get; set; }
}