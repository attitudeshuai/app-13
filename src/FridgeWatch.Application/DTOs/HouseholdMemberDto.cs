using FridgeWatch.Domain.Enums;

namespace FridgeWatch.Application.DTOs;

public class HouseholdMemberDto
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public int UserId { get; set; }
    public HouseholdRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public UserDto? User { get; set; }
    public HouseholdDto? Household { get; set; }
}

public class HouseholdMemberCreateDto
{
    public int HouseholdId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
}

public class HouseholdMemberUpdateDto
{
    public HouseholdRole? Role { get; set; }
}

public class JoinHouseholdDto
{
    public string InviteCode { get; set; } = string.Empty;
}
