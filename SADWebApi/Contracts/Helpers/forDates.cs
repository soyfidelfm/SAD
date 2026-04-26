using System;

namespace SADWebApi.Contracts.Helpers
{
	public static class DateTimeHelper
	{
		private static readonly TimeZoneInfo PstTimeZone =
			TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

		/// <summary>
		/// Returns today's start and end in UTC, based on PST business day.
		/// </summary>
		public static (DateTime StartUtc, DateTime EndUtc) GetTodayUtcFromPst()
		{
			var todayPst = TimeZoneInfo
				.ConvertTimeFromUtc(DateTime.UtcNow, PstTimeZone)
				.Date;

			var startUtc = TimeZoneInfo.ConvertTimeToUtc(todayPst, PstTimeZone);
			var endUtc = startUtc.AddDays(1);

			return (startUtc, endUtc);
		}

		/// <summary>
		/// Converts a PST date (date-only) into a UTC range (start/end).
		/// </summary>
		public static (DateTime StartUtc, DateTime EndUtc) GetUtcRangeFromPstDate(DateTime pstDate)
		{
			var dateOnlyPst = DateTime.SpecifyKind(pstDate.Date, DateTimeKind.Unspecified);

			var startUtc = TimeZoneInfo.ConvertTimeToUtc(dateOnlyPst, PstTimeZone);
			var endUtc = startUtc.AddDays(1);

			return (startUtc, endUtc);
		}

		/// <summary>
		/// Converts a PST DateTime to UTC safely.
		/// </summary>
		public static DateTime ConvertPstToUtc(DateTime pstDateTime)
		{
			var unspecified = DateTime.SpecifyKind(pstDateTime, DateTimeKind.Unspecified);
			return TimeZoneInfo.ConvertTimeToUtc(unspecified, PstTimeZone);
		}

		/// <summary>
		/// Returns today's UTC range based on PST business day (DST-safe).
		/// Does NOT affect existing helpers.
		/// </summary>
		public static (DateTime StartUtc, DateTime EndUtc) GetTodayUtcFromPst_DstSafe()
		{
			var nowUtc = DateTime.UtcNow;

			var todayPst = TimeZoneInfo
				.ConvertTimeFromUtc(nowUtc, PstTimeZone)
				.Date;

			var startUtc = TimeZoneInfo.ConvertTimeToUtc(
				DateTime.SpecifyKind(todayPst, DateTimeKind.Unspecified),
				PstTimeZone);

			var endUtc = TimeZoneInfo.ConvertTimeToUtc(
				DateTime.SpecifyKind(todayPst.AddDays(1), DateTimeKind.Unspecified),
				PstTimeZone);

			return (startUtc, endUtc);
		}
	}
}
