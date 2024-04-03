using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarWatch.Model;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class LocationControllerIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    
    public LocationControllerIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task GetLocation_UserAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestUserScheme");
        
        var response = await client.GetAsync($"/api/Location/getlocation/London");
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<CityData>();
        
        Assert.NotNull(data);
        Assert.Equal("London", data.City);
    }
    
    [Fact]
    public async Task GetLocation_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.GetAsync($"/api/Location/getlocation/Stockholm");
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<CityData>();
        
        Assert.NotNull(data);
        Assert.Equal("Stockholm", data.City);
    }
    
    [Fact]
    public async Task GetAllCities_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.GetAsync("/api/Location/getall");
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<List<CityData>>();
        
        Assert.NotNull(data);
    }
}