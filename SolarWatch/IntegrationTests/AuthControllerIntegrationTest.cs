using System.Net.Http.Json;
using SolarWatch.Contracts;
using SolarWatch.Model;
using SolarWatch.Services.AuthServices;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class AuthControllerIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    
    public AuthControllerIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task RegisterUser_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();

        var registerRequest = new RegistrationRequest("test@test.com", "test", "test123", "Test");
        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        response.EnsureSuccessStatusCode();
        var userResponse = await response.Content.ReadFromJsonAsync<ApplicationUser>();

        Assert.NotNull(userResponse);
        Assert.Equal("test", registerRequest.Username);
        Assert.Equal("test@test.com", registerRequest.Email);
        Assert.Equal("Test", registerRequest.City);
    }

    [Fact]
    public async Task LoginUser_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();

        var loginRequest = new AuthRequest("admin", "admin123");
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.Equal("admin", loginRequest.EmailOrUserName);
    }
    
    [Fact]
    public async Task LoginUser_InvalidCredentials_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();

        var loginRequest = new AuthRequest("admin", "admin1234");
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task WhoAmI_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();

        var loginRequest = new AuthRequest("admin", "admin123");
        var response = await client.PostAsJsonAsync("/api/Auth/login", loginRequest);

        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.Equal("admin", loginRequest.EmailOrUserName);

        var whoAmIResponse = await client.GetAsync("/api/Auth/whoami");
        whoAmIResponse.EnsureSuccessStatusCode();
        var whoAmI = await whoAmIResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(whoAmI);
        Assert.Equal("admin@admin.com", whoAmI.Email);
    }
}