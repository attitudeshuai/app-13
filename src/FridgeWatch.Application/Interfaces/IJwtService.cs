namespace FridgeWatch.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(int userId, string username, string email);
    int GetUserIdFromToken(string token);
}
