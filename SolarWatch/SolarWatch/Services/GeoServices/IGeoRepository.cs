using SolarWatch.Model;

namespace SolarWatch.Services.GeoServices;

public interface IGeoRepository
{
    IEnumerable<CityData> GetAllCities();
    CityData GetCity(string city);
    void AddCity(CityData city);
    string? GetLongitude(string city);
    string? GetLatitude(string city);
}