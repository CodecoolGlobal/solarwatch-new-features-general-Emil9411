using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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

    public SWController(ILogger<SWController> logger, ISWApi swApi, IJsonProcessorSW jsonProcessorSW, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo)
    {
        _logger = logger;
        _swApi = swApi;
        _jsonProcessorSW = jsonProcessorSW;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
    }

    [HttpGet("getdata")]
    public async Task<ActionResult<SWData>> GetData([Required] string city, [Required] DateOnly date)
    {
        try
        {
            var geoData = await _geoApi.GetLongLat(city);
            var cityData = _jsonProcessorGeo.LongLatProcessor(geoData);
            var data = await _swApi.GetSolarData(date, cityData.Latitude, cityData.Longitude);
            var swData = _jsonProcessorSW.SolarJsonProcessor(data);

            var newCity = new SWData
            {
                City = cityData.City,
                Date = date,
                Sunrise = swData[0],
                Sunset = swData[1]
            };

            return Ok(newCity);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error making API call for city: {city}", city);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Error processing API response for city: {city}", city);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting data for city: {city}", city);
        }
        
        return NotFound("Error getting data for city: {city}");
    }
}