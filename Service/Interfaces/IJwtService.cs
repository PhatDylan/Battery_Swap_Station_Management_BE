namespace Service.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(string userId, string role);
    bool ValidateJwtToken(string token, out string userId, out string role);
}