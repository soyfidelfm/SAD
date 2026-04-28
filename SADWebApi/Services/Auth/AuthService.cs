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
		var p1 = new NpgsqlParameter("@IdentityProviderCode", dto.IdentityProviderCode);
		var p2 = new NpgsqlParameter("@ProviderSubject", dto.ProviderSubject);
		var p3 = new NpgsqlParameter("@Email", (object?)dto.Email ?? DBNull.Value);
		var p4 = new NpgsqlParameter("@DisplayName", (object?)dto.DisplayName ?? DBNull.Value);
		var p5 = new NpgsqlParameter("@Anumber", dto.Anumber);
		var p6 = new NpgsqlParameter("@StoreId", dto.StoreId);		

		var outUserId = new NpgsqlParameter("@UserId", SqlDbType.UniqueIdentifier)
		{
			Direction = ParameterDirection.Output
		};

		await _db.Database.ExecuteSqlRawAsync(@"
    EXEC auth.UpsertUserFromExternalLogin
        @IdentityProviderCode,
        @ProviderSubject,
        @Email,
        @DisplayName,
        @StoreId,
        @Anumber,
        @UserId OUTPUT
", new[] { p1, p2, p3, p4, p6, p5, outUserId }, ct);


		return (Guid)outUserId.Value!;
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
