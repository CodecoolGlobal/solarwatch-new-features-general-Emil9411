using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.GeoServices;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeoController : ControllerBase
{
    private readonly ILogger<GeoController> _logger;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly IGeoRepository _geoRepository;
    private readonly IJsonErrorHandling _jsonErrorHandling;
    
    public GeoController(ILogger<GeoController> logger, IGeoApi geoApi, IJsonProcessorGeo jsonProcessorGeo, IGeoRepository geoRepository, IJsonErrorHandling jsonErrorHandling)
    {
        _logger = logger;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _geoRepository = geoRepository;
        _jsonErrorHandling = jsonErrorHandling;
    }
    
    [HttpGet("getlonglat"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<CityData>> GetLongLat([Required] string city)
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
            
            var json = await _geoApi.GetLongLat(city);

            var errorResult = _jsonErrorHandling.GeoJsonError(json);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var data = _jsonProcessorGeo.LongLatProcessor(json);
            
            _geoRepository.AddCity(data);
            
            return Ok(data);
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
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting long/lat for city: {city}", city);
            return BadRequest(e.Message);
        }
    }
}