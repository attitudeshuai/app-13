namespace FridgeWatch.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int? DefaultHouseholdId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserRegisterDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public class UserLoginDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserUpdateDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public string? Password { get; set; }
    public int? DefaultHouseholdId { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}
