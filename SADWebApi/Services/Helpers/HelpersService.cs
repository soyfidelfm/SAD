namespace SADWebApi.Services.Helpers
{
  public class HelpersService : IHelpers
  {
    public TimeZoneInfo GetTimeZone(string timeZone)
    {
      if (string.IsNullOrWhiteSpace(timeZone))
        throw new ArgumentException("Time zone is required.", nameof(timeZone));

      return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
    }

    public DateTime ConvertLocalToUtc(
      DateTime localDateTime,
      TimeZoneInfo tz)
    {
      var localUnspecified = DateTime.SpecifyKind(
        localDateTime,
        DateTimeKind.Unspecified);

      return TimeZoneInfo.ConvertTimeToUtc(
        localUnspecified,
        tz);
    }

    public DateTime ConvertUtcToLocal(DateTime utcDateTime, TimeZoneInfo tz)
    {
      var utc = DateTime.SpecifyKind(
          utcDateTime,
          DateTimeKind.Utc);

      return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }
  }
}
