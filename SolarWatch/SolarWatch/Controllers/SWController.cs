using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;
using SolarWatch.Services.SwServices;
using SolarWatch.Utilities;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SwController : ControllerBase
{
    private readonly ILogger<SwController> _logger;
    private readonly ISwRepository _swRepository;
    private readonly INormalizeCityName _normalizeCityName;
    private readonly ISwService _swService;

    public SwController(
        ILogger<SwController> logger, ISwRepository swRepository, INormalizeCityName normalizeCityName,
        ISwService swService)
    {
        _logger = logger;
        _swRepository = swRepository;
        _normalizeCityName = normalizeCityName;
        _swService = swService;
    }

    [HttpGet("getdata/{city}/{date}"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<SwData>> GetData([Required] string city, [Required] DateOnly date)
    {
        try
        {
            var normalizedCity = _normalizeCityName.Normalize(city);
            var swData = await _swService.GetSwData(normalizedCity, date);
            return swData;
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
    public ActionResult<IEnumerable<SwData>> GetAll()
    {
        try
        {
            var allData = _swRepository.GetAllSwDatas();
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

    [HttpPatch("update/{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<SwData>> Update([Required] int id, [FromBody] SwData updatedData)
    {
        try
        {
            var swData = await _swService.UpdateSwData(id, updatedData);
            return swData;
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating database for id: {id}", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating data for id: {id}", id);
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("delete/{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete([Required] int id)
    {
        try
        {
            var swData = await _swRepository.GetSwDataById(id);
            if (swData == null)
            {
                return NotFound();
            }

            _swRepository.DeleteSwData(id);

            return Ok();
        }
        catch (DbUpdateException e)
        {
            _logger.LogError(e, "Error updating database for id: {id}", id);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting data for id: {id}", id);
            return BadRequest(e.Message);
        }
    }
}