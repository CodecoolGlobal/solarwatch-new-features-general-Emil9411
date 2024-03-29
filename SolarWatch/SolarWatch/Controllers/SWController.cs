using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Services.GeoServices;
using SolarWatch.Services.SWServices;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SWController : ControllerBase
{
    private readonly ILogger<SWController> _logger;
    private readonly ISWApi _swApi;
    private readonly IJsonProcessorSW _jsonProcessorSW;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly ISWRepository _swRepository;
    private readonly IGeoRepository _geoRepository;

    public SWController(ILogger<SWController> logger, ISWApi swApi, IJsonProcessorSW jsonProcessorSW, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo, ISWRepository swRepository, IGeoRepository geoRepository)
    {
        _logger = logger;
        _swApi = swApi;
        _jsonProcessorSW = jsonProcessorSW;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _swRepository = swRepository;
        _geoRepository = geoRepository;
    }

    [HttpGet("getdata")]
    public async Task<ActionResult<SWData>> GetData([Required] string city, [Required] DateOnly date)
    {
        try
        {
            var swDataFromDb = _swRepository.GetSWData(city, date);
            if (swDataFromDb != null)
            {
                return Ok(swDataFromDb);
            }
            
            var geoDataFromDb = _geoRepository.GetCity(city);
            if (geoDataFromDb != null)
            {
                var solarJson = await _swApi.GetSolarData(date, geoDataFromDb.Latitude, geoDataFromDb.Longitude);
                var solarData = _jsonProcessorSW.SolarJsonProcessor(solarJson);
                
                var newCity = new SWData
                {
                    City = geoDataFromDb.City,
                    Date = date,
                    Sunrise = solarData[0],
                    Sunset = solarData[1]
                };
                
                _swRepository.AddSWData(newCity);
                
                return Ok(newCity);
            }
            
            var geoData = await _geoApi.GetLongLat(city);
            var cityData = _jsonProcessorGeo.LongLatProcessor(geoData);
            var newCityData = new CityData
            {
                City = cityData.City,
                Latitude = cityData.Latitude,
                Longitude = cityData.Longitude
            };
            
            _geoRepository.AddCity(newCityData);
            
            var json = await _swApi.GetSolarData(date, cityData.Latitude, cityData.Longitude);
            var swData = _jsonProcessorSW.SolarJsonProcessor(json);
            
            var newSolarWatchCity = new SWData
            {
                City = cityData.City,
                Date = date,
                Sunrise = swData[0],
                Sunset = swData[1]
            };
            
            _swRepository.AddSWData(newSolarWatchCity);
            
            return Ok(newSolarWatchCity);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error making API call for city: {city}", city);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error processing API response for city: {city}", city);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating database for city: {city}", city);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting data for city: {city}", city);
        }
        
        return NotFound("Error getting data for city: {city}");
    }
}