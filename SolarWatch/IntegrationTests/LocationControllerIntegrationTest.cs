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
    public async Task GetLocation_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();

        var city = "New York";
        var response = await client.GetAsync($"/api/location/getlocation?city={city}");

        response.EnsureSuccessStatusCode();
        var locationResponse = await response.Content.ReadFromJsonAsync<CityData>();

        Assert.NotNull(locationResponse);
        Assert.Equal("New York", locationResponse.City);
        Assert.Equal(40.7127281, locationResponse.Latitude);
        Assert.Equal(-74.006015199999993, locationResponse.Longitude);
    }
    
}