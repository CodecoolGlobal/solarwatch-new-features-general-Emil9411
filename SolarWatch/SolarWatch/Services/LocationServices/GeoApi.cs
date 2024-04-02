namespace SolarWatch.Services.LocationServices;

public class GeoApi : IGeoApi
{
    private readonly ILogger<GeoApi> _logger;
    private readonly IConfiguration _configuration;
    
    public GeoApi(ILogger<GeoApi> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> GetLongLat(string city)
    {
        var apiKey = _configuration.GetSection("GeoLocationApiKey").Value;
        
        var cityWithUpperCaseFirstLetter = char.ToUpper(city[0]) + city[1..];
        var url = $"https://api.openweathermap.org/geo/1.0/direct?q={cityWithUpperCaseFirstLetter}&appid={apiKey}";

        using var client = new HttpClient();
        _logger.LogInformation("Calling Geo API with url: {url}", url);

        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}