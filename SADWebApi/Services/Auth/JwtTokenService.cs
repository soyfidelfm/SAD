using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Sad.Api.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opt;

    public JwtTokenService(IOptions<JwtOptions> opt)
    {
        _opt = opt.Value;
    }

	public string CreateAccessToken(
	Guid userId,
	string? aNumber,
	string? email,
	string? displayName,
	int storeId,
	string? storeName,
	string? role)
	{
		var claims = new List<Claim>
	{
        // Mantén lo tuyo
        new(JwtRegisteredClaimNames.Sub, userId.ToString()),
		new("uid", userId.ToString()),

        // ✅ Para que User.FindFirstValue(ClaimTypes.NameIdentifier) funcione en /api/auth/me
        new(ClaimTypes.NameIdentifier, userId.ToString()),

        // ✅ Store
        new("storeId", storeId.ToString()),
		new("storeName", storeName ?? string.Empty),

        // ✅ Role (para ClaimTypes.Role)
        new(ClaimTypes.Role, string.IsNullOrWhiteSpace(role) ? "Advisor" : role!)
	};

		if (!string.IsNullOrWhiteSpace(aNumber))
			claims.Add(new Claim("aNumber", aNumber));

		if (!string.IsNullOrWhiteSpace(email))
		{
			// Mantén lo tuyo
			claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));

			// ✅ Opcional pero útil: ClaimTypes.Email (algunas libs lo esperan)
			claims.Add(new Claim(ClaimTypes.Email, email));
		}

		if (!string.IsNullOrWhiteSpace(displayName))
		{
			// Mantén lo tuyo
			claims.Add(new Claim("name", displayName));

			// ✅ Para que tu /me lea displayName si quieres (opcional)
			claims.Add(new Claim("displayName", displayName));
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var now = DateTime.UtcNow;

		var token = new JwtSecurityToken(
			issuer: _opt.Issuer,
			audience: _opt.Audience,
			claims: claims,
			notBefore: now,
			expires: now.AddMinutes(_opt.AccessTokenMinutes),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

}
