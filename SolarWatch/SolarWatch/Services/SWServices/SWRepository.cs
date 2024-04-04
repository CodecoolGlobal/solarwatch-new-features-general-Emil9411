using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.SwServices;

public class SwRepository : ISwRepository
{
    private readonly DataContext _context;
    private readonly INormalizeCityName _normalizeCityName;

    public SwRepository(DataContext context, INormalizeCityName normalizeCityName)
    {
        _context = context;
        _normalizeCityName = normalizeCityName;
    }

    public IEnumerable<SwData> GetAllSwDatas()
    {
        return _context.SolarWatchDataTable.ToList();
    }

    public SwData GetSwData(string city, DateOnly date)
    {
        city = _normalizeCityName.Normalize(city);
        return _context.SolarWatchDataTable.FirstOrDefault(c => c.City == city && c.Date == date);
    }

    public async Task<SwData> GetSwDataById(int id)
    {
        return await _context.SolarWatchDataTable.FirstOrDefaultAsync(c => c.Id == id);
    }

    public void AddSwData(SwData swData)
    {
        _context.SolarWatchDataTable.Add(swData);
        _context.SaveChanges();
    }

    public async Task UpdateSwData(SwData swData)
    {
        var swDataFromDb = await _context.SolarWatchDataTable.FirstOrDefaultAsync(c => c.Id == swData.Id);
        if (swDataFromDb != null)
        {
            swDataFromDb.City = swData.City;
            swDataFromDb.Date = swData.Date;
            swDataFromDb.Sunrise = swData.Sunrise;
            swDataFromDb.Sunset = swData.Sunset;

            _context.SolarWatchDataTable.Update(swDataFromDb);
            await _context.SaveChangesAsync();
        }
    }

    public void DeleteSwData(int id)
    {
        var swData = _context.SolarWatchDataTable.FirstOrDefault(c => c.Id == id);
        if (swData != null)
        {
            _context.SolarWatchDataTable.Remove(swData);
            _context.SaveChanges();
        }

    }

}