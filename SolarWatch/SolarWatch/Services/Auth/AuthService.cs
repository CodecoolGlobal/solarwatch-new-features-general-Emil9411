using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SolarWatch.Model;

namespace SolarWatch.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    
    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }
    
    public async Task<AuthResult> RegisterAsync(string email, string username, string password, string city, string role)
    {
        var user = new ApplicationUser
        {
            Email = email,
            UserName = username,
            City = city
        };
        var result = await _userManager.CreateAsync(user, password);
        
        if (!result.Succeeded)
        {
            return FailedRegistration(result, email, username);
        }
        
        await _userManager.AddToRoleAsync(user, role);
        return new AuthResult(true, email, username, "");
    }

    public async Task<AuthResult> LoginAsync(string emailOrUserName, string password)
    {
        if (IsValidEmail(emailOrUserName))
        {
            var user = await _userManager.FindByEmailAsync(emailOrUserName);
            if (user == null)
            {
                return InvalidUsername(emailOrUserName);
            }
            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
            {
                return InvalidPassword(emailOrUserName, user.UserName);
            }
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles[0]);
            return new AuthResult(true, emailOrUserName, user.UserName, accessToken);
        }
        else
        {
            var user = await _userManager.FindByNameAsync(emailOrUserName);
            if (user == null)
            {
                return InvalidUsername(emailOrUserName);
            }
            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
            {
                return InvalidPassword(user.Email, emailOrUserName);
            }
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles[0]);
            return new AuthResult(true, user.Email, emailOrUserName, accessToken);
        }
    }
    
    public JwtSecurityToken Verify(string token){
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration.GetSection("IssuerSigningKey").Value);
        tokenHandler.ValidateToken(token, new TokenValidationParameters{
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
        }, out SecurityToken validatedToken);
        return (JwtSecurityToken)validatedToken;
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
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private static AuthResult InvalidUsername(string username)
    {
        var result = new AuthResult(false, "", username, "");
        result.ErrorMessages.Add("Bad credentials", "Invalid username");
        return result;
    }

    private static AuthResult InvalidPassword(string email, string userName)
    {
        var result = new AuthResult(false, email, userName, "");
        result.ErrorMessages.Add("Bad credentials", "Invalid password");
        return result;
    }
    
}