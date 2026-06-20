namespace FridgeWatch.Application.DTOs;

public class HouseholdDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public DateTime? InviteCodeExpiresAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int AutoArchiveDays { get; set; }
}

public class HouseholdCreateDto
{
    public string Name { get; set; } = string.Empty;
}

public class HouseholdUpdateDto
{
    public string? Name { get; set; }
    public int? AutoArchiveDays { get; set; }
}

public class ResetInviteCodeDto
{
    public int? ValidDays { get; set; }
}
