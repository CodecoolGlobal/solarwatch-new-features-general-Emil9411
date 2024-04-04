using SolarWatch.Model;

namespace SolarWatch.Services.SwServices;

public interface ISwRepository
{
    IEnumerable<SwData> GetAllSwDatas();
    SwData GetSwData(string city, DateOnly date);
    Task<SwData> GetSwDataById(int id);
    void AddSwData(SwData swData);
    Task UpdateSwData(SwData swData);
    void DeleteSwData(int id);
}