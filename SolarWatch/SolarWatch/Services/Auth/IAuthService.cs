using System.IdentityModel.Tokens.Jwt;
using SolarWatch.Model;

namespace SolarWatch.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string username, string password, string city, string role);
    Task<AuthResult> LoginAsync(string emailOrUserName, string password);
    Task<ApplicationUser> GetUser(string email);
    JwtSecurityToken Verify(string token);
}