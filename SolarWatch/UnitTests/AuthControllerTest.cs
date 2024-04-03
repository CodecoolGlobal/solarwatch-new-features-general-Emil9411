using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SolarWatch.Contracts;
using SolarWatch.Controllers;
using SolarWatch.Services.Auth;

namespace UnitTests;

public class AuthControllerTest
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private AuthController _authController;

    private const string TestEmail = "test@test.com";
    private const string TestUsername = "test";
    private const string TestPassword = "password";
    private const string TestCity = "City";
    
    [SetUp]
    public void Setup()
    {
        _authController = new AuthController(_authServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Register_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        _authController.ModelState.AddModelError("Email", "Email is required");
        var request = new RegistrationRequest(TestEmail, TestUsername, TestPassword, TestCity);

        // Act
        var result = await _authController.Register(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Register_WhenRegistrationFails_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegistrationRequest(TestEmail, TestUsername, TestPassword, TestCity);
        _authServiceMock.Setup(x => x.RegisterAsync(TestEmail, TestUsername, TestPassword, TestCity, "User"))
            .ReturnsAsync(new AuthResult(false, TestEmail, TestUsername, "Failed to register"));

        // Act
        var result = await _authController.Register(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Register_WhenRegistrationSucceeds_ReturnsCreated()
    {
        // Arrange
        var request = new RegistrationRequest(TestEmail, TestUsername, TestPassword, TestCity);
        _authServiceMock.Setup(x => x.RegisterAsync(TestEmail, TestUsername, TestPassword, TestCity, "User"))
            .ReturnsAsync(new AuthResult(true, TestEmail, TestUsername, ""));

        // Act
        var result = await _authController.Register(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
    }
    
    [Test]
    public async Task Login_WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        _authController.ModelState.AddModelError("EmailOrUserName", "Email or username is required");
        var request = new AuthRequest(TestEmail, TestPassword);

        // Act
        var result = await _authController.Login(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Login_WhenLoginFails_ReturnsBadRequest()
    {
        // Arrange
        var request = new AuthRequest(TestEmail, TestPassword);
        _authServiceMock.Setup(x => x.LoginAsync(TestEmail, TestPassword))
            .ReturnsAsync(new AuthResult(false, TestEmail, TestUsername, "Failed to login"));

        // Act
        var result = await _authController.Login(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Login_WhenLoginSucceeds_ReturnsOk()
    {
        // Arrange
        var request = new AuthRequest(TestEmail, TestPassword);
        _authServiceMock.Setup(x => x.LoginAsync(TestEmail, TestPassword))
            .ReturnsAsync(new AuthResult(true, TestEmail, TestUsername, "token"));

        // Act
        var result = await _authController.Login(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void WhoAmI_WhenTokenIsInvalid_ReturnsNull()
    {
        // Arrange
        var requestCookie = _authController.HttpContext.Request.Cookies["Authorization"];
        _authServiceMock.Setup(x => x.Verify(requestCookie))
            .Returns((JwtSecurityToken)null);

        // Act
        var result = _authController.WhoAmI();

        // Assert
        Assert.That(result.Value, Is.Null);
    }
    
    [Test]
    public void WhoAmI_WhenTokenIsValid_ReturnsOk()
    {
        // Arrange
        var requestCookie = _authController.HttpContext.Request.Cookies["Authorization"];
        var token = new JwtSecurityToken(claims: new List<Claim>
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", TestEmail),
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", TestUsername)
        });
        _authServiceMock.Setup(x => x.Verify(requestCookie)).Returns(token);

        // Act
        var result = _authController.WhoAmI();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    
    [Test]
    public void Logout_ReturnsOk()
    {
        // Act
        var result = _authController.Logout();

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
    
}