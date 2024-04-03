using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;
using SolarWatch.Services.SWServices;
using SolarWatch.Utilities;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SwController : ControllerBase
{
    private readonly ILogger<SwController> _logger;
    private readonly ISwApi _swApi;
    private readonly IJsonProcessorSw _jsonProcessorSw;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly ISwRepository _swRepository;
    private readonly IGeoRepository _geoRepository;
    private readonly ITimeZoneApi _timeZoneApi;
    private readonly IJsonProcessorTz _jsonProcessorTz;
    private readonly IJsonErrorHandling _jsonErrorHandling;
    private readonly ICityDataCombiner _cityDataCombiner;
    private readonly INormalizeCityName _normalizeCityName;

    public SwController(ILogger<SwController> logger, ISwApi swApi, IJsonProcessorSw jsonProcessorSw, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo, ISwRepository swRepository, IGeoRepository geoRepository,
        IJsonErrorHandling jsonErrorHandling, ITimeZoneApi timeZoneApi, IJsonProcessorTz jsonProcessorTz,
        ICityDataCombiner cityDataCombiner, INormalizeCityName normalizeCityName)
    {
        _logger = logger;
        _swApi = swApi;
        _jsonProcessorSw = jsonProcessorSw;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _swRepository = swRepository;
        _geoRepository = geoRepository;
        _jsonErrorHandling = jsonErrorHandling;
        _timeZoneApi = timeZoneApi;
        _jsonProcessorTz = jsonProcessorTz;
        _cityDataCombiner = cityDataCombiner;
        _normalizeCityName = normalizeCityName;
    }

    [HttpGet("getdata/{city}/{date}"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<SwData>> GetData([Required] string city, [Required] DateOnly date)
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
            var swDataFromDb = _swRepository.GetSwData(city, date);
            if (swDataFromDb != null)
            {
                return Ok(swDataFromDb);
            }

            var geoDataFromDb = _geoRepository.GetCity(city);
            if (geoDataFromDb != null)
            {
                var solarJson = await _swApi.GetSolarData(date, geoDataFromDb.Latitude, geoDataFromDb.Longitude, geoDataFromDb.TimeZone);

                var solarJsonErrorHandlingIfCityInDb = _jsonErrorHandling.SolarJsonError(solarJson);
                if (solarJsonErrorHandlingIfCityInDb is not OkResult)
                {
                    return solarJsonErrorHandlingIfCityInDb;
                }

                var solarData = _jsonProcessorSw.SolarJsonProcessor(solarJson);

                var newCity = new SwData
                {
                    City = geoDataFromDb.City,
                    Date = date,
                    Sunrise = solarData[0],
                    Sunset = solarData[1],
                    Country = geoDataFromDb.Country,
                    TimeZone = geoDataFromDb.TimeZone
                };

                _swRepository.AddSwData(newCity);

                return Ok(newCity);
            }

            var geoData = await _geoApi.GetLongLat(city);

            var geoJsonErrorHandling = _jsonErrorHandling.GeoJsonError(geoData);
            if (geoJsonErrorHandling is not OkResult)
            {
                return geoJsonErrorHandling;
            }

            var cityData = _jsonProcessorGeo.LongLatProcessor(geoData);
            var newCityDataFromGeoApi = new CityData
            {
                City = cityData.City,
                Latitude = cityData.Latitude,
                Longitude = cityData.Longitude
            };

            var latString = cityData.Latitude.ToString(CultureInfo.InvariantCulture);
            var lonString = cityData.Longitude.ToString(CultureInfo.InvariantCulture);

            var timeZoneJson = await _timeZoneApi.GetTimeZone(latString, lonString);

            var timeZoneJsonErrorHandling = _jsonErrorHandling.TimeZoneJsonError(timeZoneJson);
            if (timeZoneJsonErrorHandling is not OkResult)
            {
                return timeZoneJsonErrorHandling;
            }

            var timeZoneData = _jsonProcessorTz.TimeZoneProcessor(timeZoneJson);
            var newCityDataFromTimeZoneApi = new CityData
            {
                TimeZone = timeZoneData.TimeZone,
                Country = timeZoneData.Country
            };

            var combinedData = _cityDataCombiner.CombineCityData(newCityDataFromGeoApi, newCityDataFromTimeZoneApi);
            
            if (combinedData.City != _normalizeCityName.Normalize(city))
            {
                combinedData.City = _normalizeCityName.Normalize(city);
            }
            
            _geoRepository.AddCity(combinedData);
            
            var solarJsonFromApi = await _swApi.GetSolarData(date, combinedData.Latitude, combinedData.Longitude, combinedData.TimeZone);
            
            var solarJsonErrorHandling = _jsonErrorHandling.SolarJsonError(solarJsonFromApi);
            if (solarJsonErrorHandling is not OkResult)
            {
                return solarJsonErrorHandling;
            }
            
            var solarDataFromApi = _jsonProcessorSw.SolarJsonProcessor(solarJsonFromApi);
            var newCityData = new SwData
            {
                City = combinedData.City,
                Date = date,
                Sunrise = solarDataFromApi[0],
                Sunset = solarDataFromApi[1],
                Country = combinedData.Country,
                TimeZone = combinedData.TimeZone
            };
            
            _swRepository.AddSwData(newCityData);
            
            return Ok(newCityData);
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
            var swData = await _swRepository.GetSwDataById(id);
            if (swData == null)
            {
                return NotFound();
            }

            // Update the fields of swData with the values from updatedData
            swData.City = updatedData.City;
            swData.Date = updatedData.Date;
            swData.Sunrise = updatedData.Sunrise;
            swData.Sunset = updatedData.Sunset;
            // Add any other fields that you want to update

            await _swRepository.UpdateSwData(swData);

            return Ok(swData);
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