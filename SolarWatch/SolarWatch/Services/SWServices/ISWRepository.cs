using SolarWatch.Model;

namespace SolarWatch.Services.SWServices;

public interface ISWRepository
{
    IEnumerable<SWData> GetAllSWDatas();
    SWData GetSWData(string city, DateOnly date);
    void AddSWData(SWData swData);
    void UpdateSWData(SWData swData);
    void DeleteSWData(SWData swData);
}