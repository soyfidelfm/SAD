namespace SADWebApi.Contracts.UserDailySettings
{
	public class UserDailySettingDto
	{
		public int Id { get; set; }

		public Guid UserId { get; set; }
		public DateTime SettingDate { get; set; }

		public decimal SalesGoalAmount { get; set; }
		public int AppsGoal { get; set; }
		public int MembershipsGoal { get; set; }

		public int StoreId { get; set; }
		public string? StoreName { get; set; }

		public bool IsActive { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
