using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public interface IGeoRepository
{
    IEnumerable<CityData> GetAllCities();
    CityData GetCity(string city);
    void AddCity(CityData city);
    void UpdateCity(CityData city);
    void DeleteCity(CityData city);
}