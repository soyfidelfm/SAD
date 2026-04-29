using System.ComponentModel.DataAnnotations;

namespace SADWebApi.Contracts.UserDailySettings
{
	public class UpdateUserDailySettingDto
	{
		[Required]
		public DateOnly SettingDate { get; set; }

		[Range(0, 9999999999.99)]
		public decimal SalesGoalAmount { get; set; }

		[Range(0, int.MaxValue)]
		public int AppsGoal { get; set; }

		[Range(0, int.MaxValue)]
		public int MembershipsGoal { get; set; }

		[Required]
		public int StoreId { get; set; }

		public bool IsActive { get; set; }
	}
}
