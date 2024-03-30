using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Contracts;
using SolarWatch.Services.Auth;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _authService.RegisterAsync(request.Email, request.Username, request.Password, request.City, "User");
        
        if (!result.Success)
        {
            AddErrors(result);
            return BadRequest(ModelState);
        }
        
        return CreatedAtAction(nameof(Register), new RegistrationResponse (result.Email, result.UserName ));
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _authService.LoginAsync(request.EmailOrUserName, request.Password);
        
        if (!result.Success)
        {
            AddErrors(result);
            return BadRequest(ModelState);
        }
        
        Response.Cookies.Append("Authorization", result.Token, new CookieOptions
        {
            HttpOnly = true
        });
        
        return Ok(new AuthResponse(result.Email, result.UserName));
    }
    
    [HttpGet("WhoAmI"), Authorize(Roles = "User,Admin")]
    public ActionResult<AuthResponse> WhoAmI()
    {
        var cookieString = Request.Cookies["Authorization"];
        
        var token = _authService.Verify(cookieString);
        
        if (token != null)
        {
            var claims = token.Claims;
            var email = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
            var username = claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
            return Ok(new AuthResponse(email, username));
        }
        return BadRequest("No token found");
    }

    [HttpPost("Logout"), Authorize(Roles = "User,Admin")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("Authorization");
        return Ok();
    }
    
    private void AddErrors(AuthResult result)
    {
        foreach (var error in result.ErrorMessages)
        {
            ModelState.AddModelError(error.Key, error.Value);
        }
    }
}