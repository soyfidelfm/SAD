namespace Sad.Api.Auth;

public interface IJwtTokenService
{
	string CreateAccessToken(
		Guid userId,
		string? aNumber,
		string? email,
		string? displayName,
		int storeId,
		string? storeName,
		string? role
	);
}
