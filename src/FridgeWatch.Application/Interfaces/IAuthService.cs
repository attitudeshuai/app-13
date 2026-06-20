using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(UserRegisterDto dto);
    Task<LoginResponseDto> LoginAsync(UserLoginDto dto);
    Task<UserDto> GetCurrentUserAsync(int userId);
    Task<UserDto> UpdateCurrentUserAsync(int userId, UserUpdateDto dto);
    Task<UserDto> SetDefaultHouseholdAsync(int userId, int? householdId);
    Task<int?> GetDefaultHouseholdIdAsync(int userId);
}
