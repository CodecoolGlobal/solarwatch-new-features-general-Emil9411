using System.Net.Http.Json;
using Moq;
using SolarWatch.Contracts;
using SolarWatch.Services.AuthServices;
using Xunit;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class AuthControllerIntegrationTest
{
    private readonly SolarWatchWebApplicationFactory _app;
    private readonly MockServices _mockServices;
    private readonly HttpClient _client;
    
    public AuthControllerIntegrationTest()
    {
        _mockServices = new MockServices();
        _app = new SolarWatchWebApplicationFactory(_mockServices);
        _client = _app.CreateClient();
        _client.DefaultRequestHeaders.Clear();
    }

    private const string Email = "test@test.com";

    [Fact]
    public async Task RegisterUser_Test()
    {
        var expected = new AuthResult(true, Email, "test", "token");
        _mockServices.AuthServiceMock.Setup(x => x.RegisterAsync(Email, "test", "test123", "Stockholm", "User"))
            .ReturnsAsync(expected);
        
        var response = await _client.PostAsJsonAsync("/api/Auth/register", new RegistrationRequest(Email, "test", "test123", "Stockholm"));
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        
        Assert.NotNull(data);
        Assert.Equal(Email, data.Email);
    }

    [Fact]
    public async Task LoginUser_Test()
    {
        var expected = new AuthResult(true, Email, "test", "token");
        _mockServices.AuthServiceMock.Setup(x => x.LoginAsync(Email, "test123"))
            .ReturnsAsync(expected);
        
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new AuthRequest(Email, "test123"));
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<AuthResponse>();
        
        Assert.NotNull(data);
        Assert.Equal(Email, data.Email);
    }
    
    [Fact]
    public async Task LoginUser_InvalidCredentials_Test()
    {
        var expected = new AuthResult(false, Email, "test", "token");
        _mockServices.AuthServiceMock.Setup(x => x.LoginAsync(Email, "test1234"))
            .ReturnsAsync(expected);
        
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new AuthRequest(Email, "test1234"));
        
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}