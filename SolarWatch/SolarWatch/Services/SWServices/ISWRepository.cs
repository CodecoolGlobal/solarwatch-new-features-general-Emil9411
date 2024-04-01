using SolarWatch.Model;

namespace SolarWatch.Services.SWServices;

public interface ISWRepository
{
    IEnumerable<SWData> GetAllSWDatas();
    SWData GetSWData(string city, DateOnly date);
    Task<SWData> GetSWDataById(int id);
    void AddSWData(SWData swData);
    Task UpdateSWData(SWData swData);
    void DeleteSWData(int id);
}