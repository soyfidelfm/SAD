namespace Sad.Api.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "sad-api";
    public string Audience { get; set; } = "sad-web";
    public string SigningKey { get; set; } = null!; // mínimo 32 chars
    public int AccessTokenMinutes { get; set; } = 60;
}
