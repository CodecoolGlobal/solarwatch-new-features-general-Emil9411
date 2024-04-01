using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarWatch.Model;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public class GeoControllerIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    
    public GeoControllerIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task GetLongLat_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.GetAsync($"/api/geo/getlonglat?city=London");
        
        response.EnsureSuccessStatusCode();
        var longLatData = await response.Content.ReadFromJsonAsync<CityData>();
        
        Assert.NotNull(longLatData);
        Assert.Equal("London", longLatData.City);
        Assert.Equal(51.507321900000001, longLatData.Latitude);
        Assert.Equal(-0.12764739999999999, longLatData.Longitude);
    }
    
}