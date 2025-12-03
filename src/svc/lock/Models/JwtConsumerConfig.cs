public sealed class JwtConsumerConfig
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
}