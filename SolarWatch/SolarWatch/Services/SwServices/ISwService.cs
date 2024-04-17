using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;

namespace SolarWatch.Services.SwServices;

public interface ISwService
{
    Task<ActionResult<SwData>> GetSwData(string city, DateOnly date);
    Task<ActionResult<SwData>> UpdateSwData(int id, SwData swData);
}