using System.Security.Claims;

namespace Sad.Api.Security;

public static class ClaimsPrincipalExtensions
{
	public static Guid GetUserIdOrThrow(this ClaimsPrincipal user)
	{
		// Usa el claim que TU token tenga.
		// Opciones comunes: ClaimTypes.NameIdentifier, "sub", "userId"
		var raw =
			user.FindFirstValue("userId") ??
			user.FindFirstValue(ClaimTypes.NameIdentifier) ??
			user.FindFirstValue("sub");

		if (string.IsNullOrWhiteSpace(raw))
			throw new UnauthorizedAccessException("Missing user id claim.");

		return Guid.Parse(raw);
	}
}
