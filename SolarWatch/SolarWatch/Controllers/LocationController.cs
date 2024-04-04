using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;
using SolarWatch.Utilities;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILogger<LocationController> _logger;
    private readonly ITimeZoneApi _timeZoneApi;
    private readonly IJsonProcessorTz _jsonProcessorTz;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly IGeoRepository _geoRepository;
    private readonly IJsonErrorHandling _jsonErrorHandling;
    private readonly ICityDataCombiner _cityDataCombiner;
    private readonly INormalizeCityName _normalizeCityName;
    
    public LocationController(ILogger<LocationController> logger, ITimeZoneApi timeZoneApi, IJsonProcessorTz jsonProcessorTz, IGeoApi geoApi, IJsonProcessorGeo jsonProcessorGeo, IGeoRepository geoRepository, IJsonErrorHandling jsonErrorHandling, ICityDataCombiner cityDataCombiner, INormalizeCityName normalizeCityName)
    {
        _logger = logger;
        _timeZoneApi = timeZoneApi;
        _jsonProcessorTz = jsonProcessorTz;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _geoRepository = geoRepository;
        _jsonErrorHandling = jsonErrorHandling;
        _cityDataCombiner = cityDataCombiner;
        _normalizeCityName = normalizeCityName;
    }
    
    [HttpGet("getlocation/{city}"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<CityData>> GetLocation([Required] string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City cannot be empty");
        }
        
        try
        {
            var dataFromDb = _geoRepository.GetCity(city);
            if (dataFromDb != null)
            {
                return Ok(dataFromDb);
            }
            
            var jsonGeo = await _geoApi.GetLongLat(city);

            var errorResult = _jsonErrorHandling.GeoJsonError(jsonGeo);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var data = _jsonProcessorGeo.LongLatProcessor(jsonGeo);
            
            var latString = data.Latitude.ToString(CultureInfo.InvariantCulture);
            var lonString = data.Longitude.ToString(CultureInfo.InvariantCulture);
            
            var jsonTz = await _timeZoneApi.GetTimeZone(latString, lonString);
            
            var errorResultTz = _jsonErrorHandling.TimeZoneJsonError(jsonTz);
            if (errorResultTz is not OkResult)
            {
                return errorResultTz;
            }
            
            var timeZone = _jsonProcessorTz.TimeZoneProcessor(jsonTz);
            
            var combinedData = _cityDataCombiner.CombineCityData(data, timeZone);
            
            if (combinedData.City != _normalizeCityName.Normalize(city))
            {
                combinedData.City = _normalizeCityName.Normalize(city);
            }
            
            _geoRepository.AddCity(combinedData);
            
            return Ok(combinedData);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Error while calling Geo API: {e}", e);
            return BadRequest(e.Message);
        }
        catch (JsonException e)
        {
            _logger.LogError("Error while processing Geo API response: {e}", e);
            return BadRequest(e.Message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError("Error while accessing database: {e}", e);
            return BadRequest(e.Message);
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
        catch (DbException e)
        {
            _logger.LogError("Error while accessing database: {e}", e);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Error while getting all cities: {e}", e);
            return BadRequest(e.Message);
        }
    }
    
    [HttpPatch("update/{id}"),Authorize(Roles = "Admin")]
    public async Task<ActionResult<CityData>> Update([Required] int id, [FromBody] CityData updatedData)
    {
        try
        {
            var cityData = await _geoRepository.GetCityById(id);
            if (cityData == null)
            {
                return NotFound();
            }
            
            cityData.City = updatedData.City;
            cityData.Latitude = updatedData.Latitude;
            cityData.Longitude = updatedData.Longitude;
            cityData.TimeZone = updatedData.TimeZone;
            cityData.Country = updatedData.Country;
            
            _geoRepository.UpdateCity(cityData);
            
            return Ok(cityData);
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