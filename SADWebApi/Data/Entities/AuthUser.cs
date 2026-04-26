namespace Sad.Api.Data.Entities;

public class AuthUser
{
    public Guid UserId { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public int? StoreId { get; set; }
    public string? Anumber { get; set; } = null!;

	public ICollection<AuthUserExternalLogin> ExternalLogins { get; set; } = new List<AuthUserExternalLogin>();
}
