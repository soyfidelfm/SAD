using SADWebApi.Contracts.UserDailySettings;

namespace SADWebApi.Services.Helpers
{
  public interface IHelpers
  {
    TimeZoneInfo GetTimeZone(string timeZone);
    DateTime ConvertLocalToUtc(DateTime localDateTime, TimeZoneInfo tz);
    DateTime ConvertUtcToLocal(DateTime utcDateTime, TimeZoneInfo tz);
  }
}
