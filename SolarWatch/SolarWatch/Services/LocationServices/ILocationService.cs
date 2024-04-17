using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public interface ILocationService
{
    Task<ActionResult<CityData?>> GetLocation(string city);
    Task<ActionResult<CityData>> UpdateLocation(int id, CityData cityData);
}