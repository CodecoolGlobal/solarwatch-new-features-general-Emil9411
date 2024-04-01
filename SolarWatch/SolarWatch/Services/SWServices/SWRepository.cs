using Microsoft.EntityFrameworkCore;
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

    public async Task<SWData> GetSWDataById(int id)
    {
        return await _context.SolarWatchDatas.FirstOrDefaultAsync(c => c.Id == id);
    }

    public void AddSWData(SWData swData)
    {
        _context.SolarWatchDatas.Add(swData);
        _context.SaveChanges();
    }

    public async Task UpdateSWData(SWData swData)
    {
        var swDataFromDb = await _context.SolarWatchDatas.FirstOrDefaultAsync(c => c.Id == swData.Id);
        if (swDataFromDb != null)
        {
            swDataFromDb.City = swData.City;
            swDataFromDb.Date = swData.Date;
            swDataFromDb.Sunrise = swData.Sunrise;
            swDataFromDb.Sunset = swData.Sunset;

            _context.SolarWatchDatas.Update(swDataFromDb);
            await _context.SaveChangesAsync();
        }
    }

    public void DeleteSWData(int id)
    {
        var swData = _context.SolarWatchDatas.FirstOrDefault(c => c.Id == id);
        if (swData != null)
        {
            _context.SolarWatchDatas.Remove(swData);
            _context.SaveChanges();
        }

    }

}