using Sad.Api.Data.Entities;

namespace SADWebApi.Data.Entities
{
	public class CatalogUserDailySetting
	{
		public int Id { get; set; }

		public Guid UserId { get; set; }
		public DateTime SettingDate { get; set; }

		public decimal SalesGoalAmount { get; set; }
		public int AppsGoal { get; set; }
		public int MembershipsGoal { get; set; }

		public int StoreId { get; set; }

		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public AuthUser User { get; set; } = null!;
		public CatalogStore Store { get; set; } = null!;
	}
}
