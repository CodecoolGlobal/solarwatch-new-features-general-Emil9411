using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;
using SolarWatch.Utilities;

namespace SolarWatch.Services.SwServices;

public class SwService : ISwService
{
    private readonly ISwApi _swApi;
    private readonly IJsonProcessorSw _jsonProcessorSw;
    private readonly ISwRepository _swRepository;
    private readonly IGeoApi _geoApi;
    private readonly IJsonProcessorGeo _jsonProcessorGeo;
    private readonly IGeoRepository _geoRepository;
    private readonly ITimeZoneApi _timeZoneApi;
    private readonly IJsonProcessorTz _jsonProcessorTz;
    private readonly IJsonErrorHandling _jsonErrorHandling;
    private readonly ICityDataCombiner _cityDataCombiner;
    private readonly INormalizeCityName _normalizeCityName;

    public SwService(ISwApi swApi, IJsonProcessorSw jsonProcessorSw, ISwRepository swRepository, IGeoApi geoApi,
        IJsonProcessorGeo jsonProcessorGeo, IGeoRepository geoRepository, ITimeZoneApi timeZoneApi,
        IJsonProcessorTz jsonProcessorTz, IJsonErrorHandling jsonErrorHandling, ICityDataCombiner cityDataCombiner,
        INormalizeCityName normalizeCityName)
    {
        _swApi = swApi;
        _jsonProcessorSw = jsonProcessorSw;
        _swRepository = swRepository;
        _geoApi = geoApi;
        _jsonProcessorGeo = jsonProcessorGeo;
        _geoRepository = geoRepository;
        _timeZoneApi = timeZoneApi;
        _jsonProcessorTz = jsonProcessorTz;
        _jsonErrorHandling = jsonErrorHandling;
        _cityDataCombiner = cityDataCombiner;
        _normalizeCityName = normalizeCityName;
    }

    public async Task<ActionResult<SwData>> GetSwData(string city, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new BadRequestObjectResult("City cannot be empty");
        }

        if (date == default)
        {
            return new BadRequestObjectResult("Date cannot be empty");
        }

        try
        {
            var dataFromDb = _swRepository.GetSwData(city, date);
            if (dataFromDb != null)
            {
                return new OkObjectResult(dataFromDb);
            }

            var cityData = _geoRepository.GetCity(city);
            if (cityData != null)
            {
                var solarJsonFromApi = await _swApi.GetSolarData(date, cityData.Latitude, cityData.Longitude, cityData.TimeZone);
                var errorResultSw = _jsonErrorHandling.SolarJsonError(solarJsonFromApi);
                if (errorResultSw is not OkResult)
                {
                    return errorResultSw;
                }

                var solarData1 = _jsonProcessorSw.SolarJsonProcessor(solarJsonFromApi);
                var solarDayLength1 = _jsonProcessorSw.DayLengthJsonProcessor(solarJsonFromApi);

                var swData = new SwData
                {
                    City = cityData.City,
                    Date = date,
                    Sunrise = solarData1[0],
                    Sunset = solarData1[1],
                    Country = cityData.Country,
                    TimeZone = cityData.TimeZone,
                    SolarNoon = solarData1[2],
                    DayLength = solarDayLength1
                };

                _swRepository.AddSwData(swData);

                return new OkObjectResult(swData);
            }

            var jsonGeo = await _geoApi.GetLongLat(city);

            var errorResult = _jsonErrorHandling.GeoJsonError(jsonGeo);
            if (errorResult is not OkResult)
            {
                return errorResult;
            }

            cityData = _jsonProcessorGeo.LongLatProcessor(jsonGeo);
            var latString = cityData.Latitude.ToString(CultureInfo.InvariantCulture);
            var lonString = cityData.Longitude.ToString(CultureInfo.InvariantCulture);

            var jsonTz = await _timeZoneApi.GetTimeZone(latString, lonString);
            var errorResultTz = _jsonErrorHandling.TimeZoneJsonError(jsonTz);
            if (errorResultTz is not OkResult)
            {
                return errorResultTz;
            }

            var timeZoneData = _jsonProcessorTz.TimeZoneProcessor(jsonTz);
            var cityDataCombined = _cityDataCombiner.CombineCityData(cityData, timeZoneData);

            if (cityDataCombined.City != _normalizeCityName.Normalize(city))
            {
                cityDataCombined.City = _normalizeCityName.Normalize(city);
            }

            _geoRepository.AddCity(cityDataCombined);

            var solarJson = await _swApi.GetSolarData(date, cityDataCombined.Latitude, cityDataCombined.Longitude,
                cityDataCombined.TimeZone);
            var errorResultSolar = _jsonErrorHandling.SolarJsonError(solarJson);
            if (errorResultSolar is not OkResult)
            {
                return errorResultSolar;
            }

            var solarData = _jsonProcessorSw.SolarJsonProcessor(solarJson);
            var solarDayLength = _jsonProcessorSw.DayLengthJsonProcessor(solarJson);

            var swDataNew = new SwData
            {
                City = cityDataCombined.City,
                Date = date,
                Sunrise = solarData[0],
                Sunset = solarData[1],
                Country = cityDataCombined.Country,
                TimeZone = cityDataCombined.TimeZone,
                SolarNoon = solarData[2],
                DayLength = solarDayLength
            };

            _swRepository.AddSwData(swDataNew);

            return new OkObjectResult(swDataNew);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }

    public async Task<ActionResult<SwData>> UpdateSwData(int id, SwData swData)
    {
        try
        {
            var dataFromDbTask = _swRepository.GetSwDataById(id);
            var dataFromDb = await dataFromDbTask;

            if (dataFromDb == null)
            {
                return new BadRequestObjectResult("Data not found");
            }

            dataFromDb.City = swData.City;
            dataFromDb.Date = swData.Date;
            dataFromDb.Sunrise = swData.Sunrise;
            dataFromDb.Sunset = swData.Sunset;
            dataFromDb.Country = swData.Country;
            dataFromDb.TimeZone = swData.TimeZone;
            dataFromDb.SolarNoon = swData.SolarNoon;
            dataFromDb.DayLength = swData.DayLength;

            await _swRepository.UpdateSwData(dataFromDb);

            return new OkObjectResult(dataFromDb);
        }
        catch (Exception e)
        {
            return new BadRequestObjectResult(e.Message);
        }
    }
}