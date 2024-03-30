using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    
    public AuthService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<AuthResult> RegisterAsync(string email, string username, string password)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return new AuthResult(false, email, username, null)
            {
                ErrorMessages = { { "email", "Email already in use" } }
            };
        }
        
        var newUser = new IdentityUser
        {
            Email = email,
            UserName = username
        };
        
        var createdUser = await _userManager.CreateAsync(newUser, password);
        return !createdUser.Succeeded ? FailedRegistration(createdUser, email, username) : new AuthResult(true, email, username, null);
    }
    
    private static AuthResult FailedRegistration(IdentityResult result, string email, string username)
    {
        var authResult = new AuthResult(false, email, username, "");

        foreach (var error in result.Errors)
        {
            authResult.ErrorMessages.Add(error.Code, error.Description);
        }

        return authResult;
    }
    
}