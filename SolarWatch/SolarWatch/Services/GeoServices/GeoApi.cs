namespace SolarWatch.Services.GeoServices;

public class GeoApi : IGeoApi
{
    private readonly ILogger<GeoApi> _logger;

    private const string ApiKey = "3201c434a51ce758b42781d96e6d2914";
    
    public GeoApi(ILogger<GeoApi> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetLongLat(string city)
    {
        var cityWithUpperCaseFirstLetter = char.ToUpper(city[0]) + city[1..];
        var url = $"https://api.openweathermap.org/geo/1.0/direct?q={cityWithUpperCaseFirstLetter}&appid={ApiKey}";

        using var client = new HttpClient();
        _logger.LogInformation("Calling Geo API with url: {url}", url);

        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}