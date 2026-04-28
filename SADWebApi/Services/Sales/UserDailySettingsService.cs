
using Microsoft.EntityFrameworkCore;
using Sad.Api.Data;
using SADWebApi.Contracts.UserDailySettings;
using SADWebApi.Data;
using SADWebApi.Data.Entities;
using SADWebApi.Services.Sales;

namespace SADWebApi.Services.Sales
{
	public class UserDailySettingsService : IUserDailySettingsService
	{
		private readonly SadDbContext _context;

		public UserDailySettingsService(SadDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<UserDailySettingDto>> GetAllAsync()
		{
			return await _context.UserDailySettings
				.AsNoTracking()
				.Include(x => x.Store)
				.OrderByDescending(x => x.SettingDate)
				.ThenBy(x => x.Id)
				.Select(x => new UserDailySettingDto
				{
					Id = x.Id,
					UserId = x.UserId,
					SettingDate = x.SettingDate,
					SalesGoalAmount = x.SalesGoalAmount,
					AppsGoal = x.AppsGoal,
					MembershipsGoal = x.MembershipsGoal,
					StoreId = x.StoreId,
					StoreName = x.Store.StoreName,
					IsActive = x.IsActive,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt
				})
				.ToListAsync();
		}

		public async Task<UserDailySettingDto?> GetByIdAsync(int id)
		{
			return await _context.UserDailySettings
				.AsNoTracking()
				.Include(x => x.Store)
				.Where(x => x.Id == id)
				.Select(x => new UserDailySettingDto
				{
					Id = x.Id,
					UserId = x.UserId,
					SettingDate = x.SettingDate,
					SalesGoalAmount = x.SalesGoalAmount,
					AppsGoal = x.AppsGoal,
					MembershipsGoal = x.MembershipsGoal,
					StoreId = x.StoreId,
					StoreName = x.Store.StoreName,
					IsActive = x.IsActive,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt
				})
				.FirstOrDefaultAsync();
		}

		public async Task<UserDailySettingDto?> GetTodaySettingsAsync()
		{
			var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

			// Hoy en Pacific Time
			var todayPacific = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone).Date;

			var startPacific = DateTime.SpecifyKind(todayPacific, DateTimeKind.Unspecified);
			var endPacific = DateTime.SpecifyKind(todayPacific.AddDays(1), DateTimeKind.Unspecified);

			// Convertimos el rango Pacific a UTC para comparar contra la DB
			var startUtc = TimeZoneInfo.ConvertTimeToUtc(startPacific, pacificZone);
			var endUtc = TimeZoneInfo.ConvertTimeToUtc(endPacific, pacificZone);

			return await _context.UserDailySettings
				.AsNoTracking()
				.Include(x => x.Store)
				.Where(x => x.SettingDate >= startUtc && x.SettingDate < endUtc)
				.OrderByDescending(x => x.SettingDate)
				.Select(x => new UserDailySettingDto
				{
					Id = x.Id,
					UserId = x.UserId,
					SettingDate = x.SettingDate,
					SalesGoalAmount = x.SalesGoalAmount,
					AppsGoal = x.AppsGoal,
					MembershipsGoal = x.MembershipsGoal,
					StoreId = x.StoreId,
					StoreName = x.Store != null ? x.Store.StoreName : null,
					IsActive = x.IsActive,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt
				})
				.FirstOrDefaultAsync();
		}

		public async Task<UserDailySettingDto?> GetTodayByUserAsync(Guid userId)
		{
			var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

			var todayPacific = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone).Date;

			var startPacific = DateTime.SpecifyKind(todayPacific, DateTimeKind.Unspecified);
			var endPacific = DateTime.SpecifyKind(todayPacific.AddDays(1), DateTimeKind.Unspecified);

			var startUtc = TimeZoneInfo.ConvertTimeToUtc(startPacific, pacificZone);
			var endUtc = TimeZoneInfo.ConvertTimeToUtc(endPacific, pacificZone);

			return await _context.UserDailySettings
				.AsNoTracking()
				.Include(x => x.Store)
				.Where(x =>
					x.UserId == userId &&
					x.IsActive &&
					x.SettingDate >= startUtc &&
					x.SettingDate < endUtc
				)
				.OrderByDescending(x => x.SettingDate)
				.Select(x => new UserDailySettingDto
				{
					Id = x.Id,
					UserId = x.UserId,
					SettingDate = x.SettingDate,
					SalesGoalAmount = x.SalesGoalAmount,
					AppsGoal = x.AppsGoal,
					MembershipsGoal = x.MembershipsGoal,
					StoreId = x.StoreId,
					StoreName = x.Store != null ? x.Store.StoreName : null,
					IsActive = x.IsActive,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt
				})
				.FirstOrDefaultAsync();
		}

		public async Task<UserDailySettingDto> CreateAsync(Guid userId, CreateUserDailySettingDto dto)
		{
			var userExists = await _context.Users.AnyAsync(x => x.UserId == userId);

			if (!userExists)
				throw new Exception("User does not exist in auth.Users.");

			var entity = new CatalogUserDailySetting
			{
				UserId = userId,
				SettingDate = dto.SettingDate.Date,
				SalesGoalAmount = dto.SalesGoalAmount,
				AppsGoal = dto.AppsGoal,
				MembershipsGoal = dto.MembershipsGoal,
				StoreId = dto.StoreId,
				IsActive = dto.IsActive,
				CreatedAt = DateTime.UtcNow
			};

			_context.UserDailySettings.Add(entity);
			await _context.SaveChangesAsync();

			var created = await _context.UserDailySettings
				.AsNoTracking()
				.Include(x => x.Store)
				.Where(x => x.Id == entity.Id)
				.Select(x => new UserDailySettingDto
				{
					Id = x.Id,
					UserId = x.UserId,
					SettingDate = x.SettingDate,
					SalesGoalAmount = x.SalesGoalAmount,
					AppsGoal = x.AppsGoal,
					MembershipsGoal = x.MembershipsGoal,
					StoreId = x.StoreId,
					StoreName = x.Store.StoreName,
					IsActive = x.IsActive,
					CreatedAt = x.CreatedAt,
					UpdatedAt = x.UpdatedAt
				})
				.FirstAsync();

			return created;
		}

		public async Task<bool> UpdateAsync(int id, UpdateUserDailySettingDto dto)
		{
			var entity = await _context.UserDailySettings.FirstOrDefaultAsync(x => x.Id == id);
			if (entity is null)
				return false;

			entity.SettingDate = dto.SettingDate.Date;
			entity.SalesGoalAmount = dto.SalesGoalAmount;
			entity.AppsGoal = dto.AppsGoal;
			entity.MembershipsGoal = dto.MembershipsGoal;
			entity.StoreId = dto.StoreId;
			entity.IsActive = dto.IsActive;
			entity.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var entity = await _context.UserDailySettings.FirstOrDefaultAsync(x => x.Id == id);
			if (entity is null)
				return false;

			_context.UserDailySettings.Remove(entity);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
