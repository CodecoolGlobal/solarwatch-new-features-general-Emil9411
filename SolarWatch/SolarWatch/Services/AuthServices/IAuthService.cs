using System.IdentityModel.Tokens.Jwt;

namespace SolarWatch.Services.AuthServices;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string username, string password, string city, string role);
    Task<AuthResult> LoginAsync(string emailOrUserName, string password);
    JwtSecurityToken Verify(string token);
}