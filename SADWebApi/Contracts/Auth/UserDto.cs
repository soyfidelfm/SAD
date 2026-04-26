public record UserDto(
	Guid UserId,
	string? DisplayName,
	string ? Email,
	bool IsActive,
	DateTime? LastLoginAtUtc,
	DateTime CreatedAtUtc,
	string? Anumber,
	int? StoreId
);