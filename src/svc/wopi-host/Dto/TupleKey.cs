using System.Text.Json.Serialization;
public sealed class TupleKey
{
    [JsonPropertyName("user")]
    public string User { get; set; } = default!;

    [JsonPropertyName("relation")]
    public string Relation { get; set; } = default!;

    [JsonPropertyName("object")]
    public string Object { get; set; } = default!;
}