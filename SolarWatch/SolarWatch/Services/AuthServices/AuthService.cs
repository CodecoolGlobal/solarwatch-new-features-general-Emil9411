using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.AuthServices;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IsValidEmail _isValidEmail = new();
    
    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }
    
    public async Task<AuthResult> RegisterAsync(string email, string username, string password, string city, string role)
    {
        if (!_isValidEmail.EmailValidation(email))
        {
            return InvalidEmail(email);
        }
        
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
        if (_isValidEmail.EmailValidation(emailOrUserName))
        {
            var user = await _userManager.FindByEmailAsync(emailOrUserName);
            if (user == null)
            {
                return InvalidUsername(emailOrUserName);
            }
            var result = await _userManager.CheckPasswordAsync(user, password);
            if (!result)
            {
                return InvalidPassword(emailOrUserName, user?.UserName ?? ""); // Added null check and default value for userName
            }
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles[0]);
            return new AuthResult(true, emailOrUserName, user?.UserName ?? "", accessToken); // Added null check and default value for userName
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
                return InvalidPassword(user?.Email ?? "", emailOrUserName); // Added null check and default value for email
            }
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.CreateToken(user, roles[0]);
            return new AuthResult(true, user?.Email ?? "", emailOrUserName, accessToken); // Added null check and default value for email
        }
    }
    
    public JwtSecurityToken Verify(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var issuerSigningKey = _configuration.GetSection("IssuerSigningKey").Value;
        var key = !string.IsNullOrEmpty(issuerSigningKey) ? Encoding.UTF8.GetBytes(issuerSigningKey) : throw new ArgumentNullException("IssuerSigningKey is null or empty.");
        
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
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
    
    private static AuthResult InvalidEmail(string email)
    {
        var result = new AuthResult(false, email, "", "");
        result.ErrorMessages.Add("Bad credentials", "Invalid email");
        return result;
    }
    
}