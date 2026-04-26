namespace Sad.Api.Contracts.Auth;

public record ExternalLoginRequestDto(
    string IdentityProviderCode,
    string ProviderSubject,
    string? Email,
    string? DisplayName,
    string? Anumber,
    int? StoreId
);
