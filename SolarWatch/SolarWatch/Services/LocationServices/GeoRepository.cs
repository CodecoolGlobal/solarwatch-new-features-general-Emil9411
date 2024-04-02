using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.LocationServices;

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
        return _context.CityDataTable.ToList();
    }
    
    public CityData GetCity(string city)
    {
        city = _normalizeCityName.Normalize(city);
        return _context.CityDataTable.FirstOrDefault(c => c.City == city);
    }
    
    public void AddCity(CityData city)
    {
        _context.CityDataTable.Add(city);
        _context.SaveChanges();
    }
    
    public void UpdateCity(CityData city)
    {
        _context.CityDataTable.Update(city);
        _context.SaveChanges();
    }
    
    public void DeleteCity(CityData city)
    {
        _context.CityDataTable.Remove(city);
        _context.SaveChanges();
    }
}