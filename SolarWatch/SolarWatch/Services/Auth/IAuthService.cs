namespace SolarWatch.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string username, string password);
}