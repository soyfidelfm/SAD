namespace Sad.Api.Data.Entities;

public class AuthUserExternalLogin
{
    public long UserExternalLoginId { get; set; }
    public Guid UserId { get; set; }
    public byte IdentityProviderId { get; set; }
    public string ProviderSubject { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }

    public AuthUser User { get; set; } = null!;
    public CatalogIdentityProvider IdentityProvider { get; set; } = null!;
}
