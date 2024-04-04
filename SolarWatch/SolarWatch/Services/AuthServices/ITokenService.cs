using SolarWatch.Model;

namespace SolarWatch.Services.AuthServices;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, string role);
}