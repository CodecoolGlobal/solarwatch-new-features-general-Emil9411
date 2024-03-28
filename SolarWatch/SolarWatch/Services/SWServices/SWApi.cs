namespace SolarWatch.Services.SWServices;

public class SWApi : ISWApi
{
    private readonly ILogger<SWApi> _logger;
    
    public SWApi(ILogger<SWApi> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> GetSolarData(DateOnly date, double latitude, double longitude)
    {
        var url = $"https://api.sunrise-sunset.org/json?lat={latitude}&lng={longitude}&date={date:yyyy-MM-dd}";
        
        using var client = new HttpClient();
        _logger.LogInformation("Calling SW API with url: {url}", url);
        
        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    } 
}