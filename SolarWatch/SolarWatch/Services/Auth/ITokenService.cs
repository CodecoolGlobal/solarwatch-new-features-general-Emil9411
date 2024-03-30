using SolarWatch.Model;

namespace SolarWatch.Services.Auth;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, string role);
}