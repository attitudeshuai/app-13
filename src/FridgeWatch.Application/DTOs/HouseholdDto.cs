namespace FridgeWatch.Application.DTOs;

public class HouseholdDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}

public class HouseholdCreateDto
{
    public string Name { get; set; } = string.Empty;
}

public class HouseholdUpdateDto
{
    public string? Name { get; set; }
}
