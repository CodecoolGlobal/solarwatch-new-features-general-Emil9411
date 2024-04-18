using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILogger<LocationController> _logger;
    private readonly ILocationService _locationService;
    private readonly IGeoRepository _geoRepository;

    public LocationController(ILogger<LocationController> logger, ILocationService locationService,
        IGeoRepository geoRepository)
    {
        _logger = logger;
        _locationService = locationService;
        _geoRepository = geoRepository;
    }

    [HttpGet("getlocation/{city}"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<CityData>> GetLocation([Required] string city)
    {
        try
        {
            var cityData = await _locationService.GetLocation(city);
            return cityData;
        }
        catch (Exception e)
        {
            _logger.LogError("Error while getting location data: {e}", e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("getall"), Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<CityData>> GetAllCities()
    {
        try
        {
            var cities = _geoRepository.GetAllCities();
            return Ok(cities);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while getting all cities: {e}", e);
            return BadRequest(e.Message);
        }
    }

    [HttpPatch("update/{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<CityData>> Update([Required] int id, [FromBody] CityData updatedData)
    {
        try
        {
            var cityData = await _locationService.UpdateLocation(id, updatedData);
            return cityData;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Error while updating database: {e}", e);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while updating city data: {e}", e);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("delete/{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([Required] int id)
    {
        try
        {
            var cityData = await _geoRepository.GetCityById(id);
            if (cityData == null)
            {
                return NotFound();
            }

            _geoRepository.DeleteCity(cityData);

            return Ok();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Error while updating database: {e}", e);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while deleting city data: {e}", e);
            return BadRequest(e.Message);
        }
    }
}