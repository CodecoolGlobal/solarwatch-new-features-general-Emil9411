using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.ErrorHandling;
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
    private readonly IJsonErrorHandling _jsonErrorHandling;

    public SWController(ILogger<SWController> logger, ISWApi swApi, IJsonProcessorSW jsonProcessorSW, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo, ISWRepository swRepository, IGeoRepository geoRepository, IJsonErrorHandling jsonErrorHandling)
    {
        _logger = logger;
        _swApi = swApi;
        _jsonProcessorSW = jsonProcessorSW;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _swRepository = swRepository;
        _geoRepository = geoRepository;
        _jsonErrorHandling = jsonErrorHandling;
    }

    [HttpGet("getdata/{city}/{date}"), Authorize(Roles = "User,Admin")]
    public async Task<ActionResult<SWData>> GetData([Required] string city, [Required] DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("City is required");
        }
        
        if (date == default)
        {
            return BadRequest("Date is required");
        }
        
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
                
                var solarJsonErrorHandlingIfCityInDb = _jsonErrorHandling.SolarJsonError(solarJson);
                if (solarJsonErrorHandlingIfCityInDb is not OkResult)
                {
                    return solarJsonErrorHandlingIfCityInDb;
                }
                
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
            
            var solarJsonErrorHandlingIfCityNotInDb = _jsonErrorHandling.SolarJsonError(json);
            if (solarJsonErrorHandlingIfCityNotInDb is not OkResult)
            {
                return solarJsonErrorHandlingIfCityNotInDb;
            }
            
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
            return BadRequest(e.Message);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error processing API response for city: {city}", city);
            return BadRequest(e.Message);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating database for city: {city}", city);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting data for city: {city}", city);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("getall"), Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<SWData>> GetAll()
    {
        try
        {
            var allData = _swRepository.GetAllSWDatas();
            return Ok(allData);
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating database");
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting all data");
            return BadRequest(e.Message);
        }
    }
}