using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.GeoServices;

public class GeoRepository : IGeoRepository
{
    private readonly DataContext _context;
    private readonly INormalizeCityName _normalizeCityName;
    
    public GeoRepository(DataContext context, INormalizeCityName normalizeCityName)
    {
        _context = context;
        _normalizeCityName = normalizeCityName;
    }
    
    public IEnumerable<CityData> GetAllCities()
    {
        return _context.CityDatas.ToList();
    }
    
    public CityData GetCity(string city)
    {
        city = _normalizeCityName.Normalize(city);
        return _context.CityDatas.FirstOrDefault(c => c.City == city);
    }
    
    public void AddCity(CityData city)
    {
        _context.CityDatas.Add(city);
        _context.SaveChanges();
    }

    public string? GetLongitude(string city)
    {
        city = _normalizeCityName.Normalize(city);
        var cityData = GetCity(city);
        if (cityData != null)
        {
            return cityData.Longitude.ToString();
        }

        return null;
    }
    
    public string? GetLatitude(string city)
    {
        city = _normalizeCityName.Normalize(city);
        var cityData = GetCity(city);
        if (cityData != null)
        {
            return cityData.Latitude.ToString();
        }

        return null;
    }
}