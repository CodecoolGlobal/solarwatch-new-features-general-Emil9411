namespace SolarWatch.Services.LocationServices;

public interface ITimeZoneApi
{
    Task<string> GetTimeZone(string lat, string lon);
}