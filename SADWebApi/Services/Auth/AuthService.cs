using Npgsql;
using Microsoft.EntityFrameworkCore;
using Sad.Api.Contracts.Auth;
using Sad.Api.Data;
using System.Data;

namespace Sad.Api.Services.Auth;

public class AuthService : IAuthService
{
	private readonly SadDbContext _db;

	public AuthService(SadDbContext db) => _db = db;

  public async Task<Guid> UpsertUserFromExternalLoginAsync(
    ExternalLoginRequestDto dto,
    CancellationToken ct)
  {
    var userId = await _db.Database
        .SqlQueryRaw<Guid>(@"
            SELECT auth.upsert_user_from_external_login(
                @IdentityProviderCode,
                @ProviderSubject,
                @Email,
                @DisplayName,
                @StoreId,
                @Anumber
            ) AS ""Value""
        ",
        new NpgsqlParameter("@IdentityProviderCode", dto.IdentityProviderCode),
        new NpgsqlParameter("@ProviderSubject", dto.ProviderSubject),
        new NpgsqlParameter("@Email", (object?)dto.Email ?? DBNull.Value),
        new NpgsqlParameter("@DisplayName", (object?)dto.DisplayName ?? DBNull.Value),
        new NpgsqlParameter("@StoreId", dto.StoreId),
        new NpgsqlParameter("@Anumber", dto.Anumber))
        .FirstAsync(ct);

    return userId;
  }

  public async Task<UserDto?> GetUserByIdAsync(
		Guid userId,
		CancellationToken ct)
	{
		return await _db.Users
			.Where(u => u.UserId == userId)
			.Select(u => new UserDto(
				u.UserId,
				u.DisplayName,
				u.Email,
				u.IsActive,
				u.LastLoginAtUtc,
				u.CreatedAtUtc,
				u.Anumber,
				u.StoreId
			))
			.SingleOrDefaultAsync(ct);
	}

	public async Task<UserDto?> UpdateUserAsync(
		UserDto dto,
		CancellationToken ct)
	{
		var user = await _db.Users
			.SingleOrDefaultAsync(u => u.UserId == dto.UserId, ct);

		if (user is null) return null;

		// ✔ SOLO CAMPOS EDITABLES
		user.DisplayName = dto.DisplayName;
		user.Email = dto.Email;
		user.IsActive = dto.IsActive;
		user.Anumber = dto.Anumber??"";
		user.StoreId = dto.StoreId;

		// ❌ NO tocar fechas
		// user.CreatedAtUtc
		// user.LastLoginAtUtc

		await _db.SaveChangesAsync(ct);

		return new UserDto(
			user.UserId,
			user.DisplayName,
			user.Email,
			user.IsActive,
			user.LastLoginAtUtc,
			user.CreatedAtUtc,
			user.Anumber,
			user.StoreId
		);
	}
}
