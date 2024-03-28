using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Services.GeoServices;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeoController : ControllerBase
{
    private readonly ILogger<GeoController> _logger;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    
    public GeoController(ILogger<GeoController> logger, IGeoApi geoApi, IJsonProcessorGeo jsonProcessorGeo)
    {
        _logger = logger;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
    }
    
    [HttpGet("getlonglat")]
    public async Task<ActionResult<double[]>> GetLongLat([Required] string city)
    {
        try
        {
            var data = await _geoApi.GetLongLat(city);
            var longLat = _jsonProcessorGeo.LongLatProcessor(data);
            return Ok(longLat);
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
            _logger.LogError(e, "Error getting long/lat for city: {city}", city);
        }
        
        return BadRequest();
    }
}