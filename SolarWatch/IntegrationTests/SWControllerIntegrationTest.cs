using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarWatch.Model;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

[Collection("IntegrationTests")]
public class SwControllerIntegrationTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    
    public SwControllerIntegrationTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public async Task GetData_UserAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestUserScheme");
        
        var response = await client.GetAsync($"/api/SW/getdata/Stockholm/2022-01-01");
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<SwData>();
        
        Assert.NotNull(data);
        Assert.Equal("Stockholm", data.City);
        Assert.Equal(new DateOnly(2022, 01, 01), data.Date);
    }
    
    [Fact]
    public async Task GetData_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.GetAsync($"/api/SW/getdata/Stockholm/2022-01-01");
        
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<SwData>();
        
        Assert.NotNull(data);
        Assert.Equal("Stockholm", data.City);
        Assert.Equal(new DateOnly(2022, 01, 01), data.Date);
    }
    
    [Fact]
    public async Task GetAll_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.GetAsync($"/api/SW/getAll");
        
        response.EnsureSuccessStatusCode();
        var allData = await response.Content.ReadFromJsonAsync<List<SwData>>();
        
        Assert.NotNull(allData);
        Assert.Empty(allData);
    }
    
    [Fact]
    public async Task Delete_AdminAuthorized_Test()
    {
        var app = new SolarWatchWebApplicationFactory();
        var client = app.CreateClient();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestAdminScheme");
        
        var response = await client.DeleteAsync($"/api/SW/delete/1");
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
}