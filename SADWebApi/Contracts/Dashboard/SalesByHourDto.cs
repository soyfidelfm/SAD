namespace SADWebApi.Contracts.Dashboard
{
	public class SalesByHourDto
	{
		public int Hour { get; set; }
		public string HourLabel { get; set; } = string.Empty;
		public decimal Total { get; set; }
	}
}
