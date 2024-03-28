namespace SolarWatch.Services.SWServices;

public interface ISWApi
{
    Task<string> GetSolarData(DateOnly date, double latitude, double longitude);
}