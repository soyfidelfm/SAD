using System.ComponentModel.DataAnnotations;

namespace SADWebApi.Contracts.UserDailySettings
{
	public class CreateUserDailySettingDto
	{
		public DateTime SettingDate { get; set; }
		public decimal SalesGoalAmount { get; set; }
		public int AppsGoal { get; set; }
		public int MembershipsGoal { get; set; }
		public int StoreId { get; set; }
		public bool IsActive { get; set; }
	}
}
