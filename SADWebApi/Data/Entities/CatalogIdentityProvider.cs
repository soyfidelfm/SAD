namespace Sad.Api.Data.Entities;

public class CatalogIdentityProvider
{
    public byte IdentityProviderId { get; set; }
    public string ProviderCode { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public bool IsActive { get; set; }

    public ICollection<AuthUserExternalLogin> UserExternalLogins { get; set; } = new List<AuthUserExternalLogin>();
}
