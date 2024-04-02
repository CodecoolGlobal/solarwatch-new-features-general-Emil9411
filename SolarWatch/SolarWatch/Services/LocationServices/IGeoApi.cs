namespace SolarWatch.Services.LocationServices;

public interface IGeoApi
{
    Task<string> GetLongLat(string city);
}