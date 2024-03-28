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
    private readonly DataContext _context;

    public SWController(ILogger<SWController> logger, ISWApi swApi, IJsonProcessorSW jsonProcessorSW, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo, DataContext context)
    {
        _logger = logger;
        _swApi = swApi;
        _jsonProcessorSW = jsonProcessorSW;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _context = context;
    }

    [HttpGet("getdata")]
    public async Task<ActionResult<SWData>> GetData([Required] string city, [Required] DateOnly date)
    {
        try
        {
            var swDataFromDb = _context.SolarWatchDatas.FirstOrDefault(c => c.City == city && c.Date == date);
            if (swDataFromDb != null)
            {
                return Ok(swDataFromDb);
            }
            
            var geoDataFromDb = _context.CityDatas.FirstOrDefault(c => c.City == city);
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
                
                _context.SolarWatchDatas.Add(newCity);
                await _context.SaveChangesAsync();
                
                return Ok(newCity);
            }
            
            var geoData = await _geoApi.GetLongLat(city);
            var cityData = _jsonProcessorGeo.LongLatProcessor(geoData);
            
            _context.CityDatas.Add(new CityData
            {
                City = cityData.City,
                Latitude = cityData.Latitude,
                Longitude = cityData.Longitude
            });
            await _context.SaveChangesAsync();
            
            var json = await _swApi.GetSolarData(date, cityData.Latitude, cityData.Longitude);
            var swData = _jsonProcessorSW.SolarJsonProcessor(json);
            
            var newSolarWatchCity = new SWData
            {
                City = cityData.City,
                Date = date,
                Sunrise = swData[0],
                Sunset = swData[1]
            };
            
            _context.SolarWatchDatas.Add(newSolarWatchCity);
            await _context.SaveChangesAsync();
            
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