using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.SWServices;

public class SWRepository : ISWRepository
{
    private readonly DataContext _context;
    private readonly INormalizeCityName _normalizeCityName;
    
    public SWRepository(DataContext context, INormalizeCityName normalizeCityName)
    {
        _context = context;
        _normalizeCityName = normalizeCityName;
    }
    
    public IEnumerable<SWData> GetAllSWDatas()
    {
        return _context.SolarWatchDatas.ToList();
    }
    
    public SWData GetSWData(string city, DateOnly date)
    {
        city = _normalizeCityName.Normalize(city);
        return _context.SolarWatchDatas.FirstOrDefault(c => c.City == city && c.Date == date);
    }
    
    public void AddSWData(SWData swData)
    {
        _context.SolarWatchDatas.Add(swData);
        _context.SaveChanges();
    }
    
    public void UpdateSWData(SWData swData)
    {
        _context.SolarWatchDatas.Update(swData);
        _context.SaveChanges();
    }
    
    public void DeleteSWData(SWData swData)
    {
        _context.SolarWatchDatas.Remove(swData);
        _context.SaveChanges();
    }
    
}