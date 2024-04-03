using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public interface IGeoRepository
{
    IEnumerable<CityData> GetAllCities();
    CityData GetCity(string city);
    Task<CityData> GetCityById(int id);
    void AddCity(CityData city);
    void UpdateCity(CityData city);
    void DeleteCity(CityData city);
}