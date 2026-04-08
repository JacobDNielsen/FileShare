using System.Text.Json.Serialization;

public sealed class CheckRequest
{
    [JsonPropertyName("authorization_model_id")]
    public string AuthorizationModelId { get; set; } = default!;

    [JsonPropertyName("tuple_key")]
    public TupleKey TupleKey { get; set; } = default!;
}