using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Data;
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
    private readonly DataContext _context;
    
    public GeoController(ILogger<GeoController> logger, IGeoApi geoApi, IJsonProcessorGeo jsonProcessorGeo, DataContext context)
    {
        _logger = logger;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _context = context;
    }
    
    [HttpGet("getlonglat")]
    public async Task<ActionResult<CityData>> GetLongLat([Required] string city)
    {
        try
        {
            var dataFromDb = _context.CityDatas.FirstOrDefault(c => c.City == city);
            if (dataFromDb != null)
            {
                return Ok(dataFromDb);
            }
            
            var json = await _geoApi.GetLongLat(city);
            var data = _jsonProcessorGeo.LongLatProcessor(json);
            
            _context.CityDatas.Add(data);
            await _context.SaveChangesAsync();
            
            return Ok(data);
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