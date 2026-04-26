using Sad.Api.Contracts.Auth;

namespace Sad.Api.Services.Auth;

public interface IAuthService
{
    Task<Guid> UpsertUserFromExternalLoginAsync(ExternalLoginRequestDto dto, CancellationToken ct);
	Task<UserDto?> GetUserByIdAsync(
		Guid userId,
		CancellationToken ct);

	Task<UserDto?> UpdateUserAsync(
		UserDto dto,
		CancellationToken ct);
}
