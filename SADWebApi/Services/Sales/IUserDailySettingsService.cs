using SADWebApi.Contracts.UserDailySettings;

namespace SADWebApi.Services.Sales
{
	public interface IUserDailySettingsService
	{
		Task<IEnumerable<UserDailySettingDto>> GetAllAsync();
		Task<UserDailySettingDto?> GetByIdAsync(int id);
		Task<UserDailySettingDto?> GetTodaySettingsAsync();
		Task<UserDailySettingDto?> GetTodayByUserAsync(Guid userId);
		Task<UserDailySettingDto> CreateAsync(Guid userId, CreateUserDailySettingDto dto);
		Task<bool> UpdateAsync(int id, UpdateUserDailySettingDto dto);
		Task<bool> DeleteAsync(int id);
	}
}