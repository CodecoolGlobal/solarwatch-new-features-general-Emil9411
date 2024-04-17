using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.LocationServices;

public class LocationService : ILocationService
{
    private readonly ITimeZoneApi _timeZoneApi;
    private readonly IJsonProcessorTz _jsonProcessorTz;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly IGeoRepository _geoRepository;
    private readonly IJsonErrorHandling _jsonErrorHandling;
    private readonly ICityDataCombiner _cityDataCombiner;
    private readonly INormalizeCityName _normalizeCityName;

    public LocationService(
        ITimeZoneApi timeZoneApi, IJsonProcessorTz jsonProcessorTz, IGeoApi geoApi, IJsonProcessorGeo jsonProcessorGeo,
        IGeoRepository geoRepository, IJsonErrorHandling jsonErrorHandling, ICityDataCombiner cityDataCombiner,
        INormalizeCityName normalizeCityName)
    {
        _timeZoneApi = timeZoneApi;
        _jsonProcessorTz = jsonProcessorTz;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _geoRepository = geoRepository;
        _jsonErrorHandling = jsonErrorHandling;
        _cityDataCombiner = cityDataCombiner;
        _normalizeCityName = normalizeCityName;
    }

    public async Task<ActionResult<CityData?>> GetLocation(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new BadRequestObjectResult("City cannot be empty");
        }

        try
        {
            var dataFromDb = _geoRepository.GetCity(city);
            if (dataFromDb != null)
            {
                return dataFromDb;
            }

            var jsonGeo = await _geoApi.GetLongLat(city);

            var errorResult = _jsonErrorHandling.GeoJsonError(jsonGeo);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var cityData = _jsonProcessorGeo.LongLatProcessor(jsonGeo);

            var latString = cityData.Latitude.ToString(CultureInfo.InvariantCulture);
            var lonString = cityData.Longitude.ToString(CultureInfo.InvariantCulture);

            var jsonTz = await _timeZoneApi.GetTimeZone(latString, lonString);

            errorResult = _jsonErrorHandling.TimeZoneJsonError(jsonTz);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var timeZoneData = _jsonProcessorTz.TimeZoneProcessor(jsonTz);

            var cityDataCombined = _cityDataCombiner.CombineCityData(cityData, timeZoneData);

            if (cityDataCombined.City != _normalizeCityName.Normalize(city))
            {
                cityDataCombined.City = _normalizeCityName.Normalize(city);
            }

            _geoRepository.AddCity(cityDataCombined);

            return new OkObjectResult(cityDataCombined);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }

    }

    public async Task<ActionResult<CityData>> UpdateLocation(int id, CityData cityData)
    {
        if (id != cityData.Id)
        {
            return new BadRequestObjectResult("Id does not match");
        }

        try
        {
            var jsonGeo = await _geoApi.GetLongLat(cityData.City);

            var errorResult = _jsonErrorHandling.GeoJsonError(jsonGeo);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var cityDataGeo = _jsonProcessorGeo.LongLatProcessor(jsonGeo);

            var latString = cityDataGeo.Latitude.ToString(CultureInfo.InvariantCulture);
            var lonString = cityDataGeo.Longitude.ToString(CultureInfo.InvariantCulture);

            var jsonTz = await _timeZoneApi.GetTimeZone(latString, lonString);

            errorResult = _jsonErrorHandling.TimeZoneJsonError(jsonTz);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            var timeZoneData = _jsonProcessorTz.TimeZoneProcessor(jsonTz);

            var cityDataCombined = _cityDataCombiner.CombineCityData(cityDataGeo, timeZoneData);

            if (cityDataCombined.City != _normalizeCityName.Normalize(cityData.City))
            {
                cityDataCombined.City = _normalizeCityName.Normalize(cityData.City);
            }

            _geoRepository.UpdateCity(cityDataCombined);

            return new OkObjectResult(cityDataCombined);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }



}
