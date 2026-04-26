using Microsoft.AspNetCore.Mvc;
using SADWebApi.Contracts.UserDailySettings;
using SADWebApi.Services.Sales;
using System.Security.Claims;

namespace SADWebApi.Controllers
{
	[ApiController]
	[Route("api/user-daily-settings")]
	public class UserDailySettingsController : ControllerBase
	{
		private readonly IUserDailySettingsService _service;

		public UserDailySettingsController(IUserDailySettingsService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserDailySettingDto>>> GetAll()
		{
			var items = await _service.GetAllAsync();
			return Ok(items);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<UserDailySettingDto>> GetById(int id)
		{
			var item = await _service.GetByIdAsync(id);

			if (item is null)
				return NotFound();

			return Ok(item);
		}

		[HttpGet("today")]
		public async Task<ActionResult<UserDailySettingDto>> GetTodayByUser()
		{
			var userIdRaw = User.FindFirst("uid")?.Value
					?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrWhiteSpace(userIdRaw) || !Guid.TryParse(userIdRaw, out var userId))
				return Unauthorized("Invalid or missing user id in token.");
			var item = await _service.GetTodayByUserAsync(userId);

			if (item is null)
				return NotFound();

			return Ok(item);
		}

		[HttpPost]
		public async Task<ActionResult<UserDailySettingDto>> Create(CreateUserDailySettingDto dto)
		{
			var userIdRaw = User.FindFirst("uid")?.Value
				?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrWhiteSpace(userIdRaw) || !Guid.TryParse(userIdRaw, out var userId))
				return Unauthorized("Invalid or missing user id in token.");

			var result = await _service.CreateAsync(userId, dto);

			return Ok(result);
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDailySettingDto dto)
		{
			var updated = await _service.UpdateAsync(id, dto);

			if (!updated)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			var deleted = await _service.DeleteAsync(id);

			if (!deleted)
				return NotFound();

			return NoContent();
		}
	}
}