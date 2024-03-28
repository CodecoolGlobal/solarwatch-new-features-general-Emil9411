namespace SolarWatch.Services.GeoServices;

public interface IGeoApi
{
    Task<string> GetLongLat(string city);
}