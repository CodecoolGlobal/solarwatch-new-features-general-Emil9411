namespace SolarWatch.Services.SWServices;

public class SwApi : ISwApi
{
    private readonly ILogger<SwApi> _logger;
    
    public SwApi(ILogger<SwApi> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> GetSolarData(DateOnly date, double latitude, double longitude, string timeZone)
    {
        var url = $"https://api.sunrise-sunset.org/json?lat={latitude}&lng={longitude}&date={date:yyyy-MM-dd}&tzid={timeZone}";
        
        using var client = new HttpClient();
        _logger.LogInformation("Calling SW API with url: {url}", url);
        
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    } 
}