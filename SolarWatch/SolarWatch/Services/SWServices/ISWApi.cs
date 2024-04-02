namespace SolarWatch.Services.SWServices;

public interface ISwApi
{
    Task<string> GetSolarData(DateOnly date, double latitude, double longitude, string timeZone);
}