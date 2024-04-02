namespace SolarWatch.Services.LocationServices;

public class TimeZoneApi : ITimeZoneApi
{
    private readonly ILogger<TimeZoneApi> _logger;
    private readonly IConfiguration _configuration;
    
    public TimeZoneApi(ILogger<TimeZoneApi> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task<string> GetTimeZone(string lat, string lon)
    {
        var apiKey = _configuration.GetSection("TimeZoneDbApiKey").Value;
        
        var url = $"https://api.timezonedb.com/v2.1/get-time-zone?key={apiKey}&format=json&by=position&lat={lat}&lng={lon}";
        
        using var client = new HttpClient();
        _logger.LogInformation("Calling TimeZone API with url: {url}", url);
        
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}