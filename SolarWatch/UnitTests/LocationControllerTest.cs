using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SolarWatch.Controllers;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.LocationServices;
using SolarWatch.Utilities;

namespace UnitTests;

public class LocationControllerTest
{
    private readonly Mock<ILogger<LocationController>> _logger = new();
    private readonly Mock<ITimeZoneApi> _timeZoneApi = new();
    private readonly Mock<IJsonProcessorTz> _jsonProcessorTz = new();
    private readonly Mock<IGeoApi> _geoApi = new();
    private readonly Mock<IJsonProcessorGeo> _jsonProcessorGeo = new();
    private readonly Mock<IGeoRepository> _geoRepository = new();
    private readonly Mock<IJsonErrorHandling> _jsonErrorHandling = new();
    private readonly Mock<ICityDataCombiner> _cityDataCombiner = new();
    private readonly Mock<INormalizeCityName> _normalizeCityName = new();

    private readonly LocationController _locationController;

    public LocationControllerTest()
    {
        _locationController = new LocationController(_logger.Object, _timeZoneApi.Object, _jsonProcessorTz.Object,
            _geoApi.Object, _jsonProcessorGeo.Object, _geoRepository.Object, _jsonErrorHandling.Object,
            _cityDataCombiner.Object, _normalizeCityName.Object);
    }
    
    private const string City = "New York";
    
    // get location endpoint tests
    [Test]
    public async Task GetLocation_WhenCityIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var city = string.Empty;
        
        // Act
        var result = await _locationController.GetLocation(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_WhenCityIsNotEmptyAndDataIsInDb_ReturnsOk()
    {
        // Arrange
        var cityData = new CityData();
        _geoRepository.Setup(x => x.GetCity(City)).Returns(cityData);
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_WhenCityIsNotEmptyAndDataIsNotInDbAndGeoApiReturnsError_ReturnsError()
    {
        // Arrange
        var jsonGeo = "error";
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ReturnsAsync(jsonGeo);
        _jsonErrorHandling.Setup(x => x.GeoJsonError(jsonGeo)).Returns(new BadRequestObjectResult("error"));
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_WhenCityIsNotEmptyAndDataIsNotInDbAndGeoApiReturnsDataAndTimeZoneApiReturnsError_ReturnsError()
    {
        // Arrange
        var jsonGeo = "data";
        var jsonTz = "error";
        var data = new CityData();
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ReturnsAsync(jsonGeo);
        _jsonErrorHandling.Setup(x => x.GeoJsonError(jsonGeo)).Returns(new OkResult());
        _jsonProcessorGeo.Setup(x => x.LongLatProcessor(jsonGeo)).Returns(data);
        _timeZoneApi.Setup(x => x.GetTimeZone(data.Latitude.ToString(), data.Longitude.ToString())).ReturnsAsync(jsonTz);
        _jsonErrorHandling.Setup(x => x.TimeZoneJsonError(jsonTz)).Returns(new BadRequestObjectResult("error"));
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_HttpRequestExceptionIsThrown_ReturnsBadRequest()
    {
        // Arrange
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ThrowsAsync(new HttpRequestException());
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_JsonExceptionIsThrown_ReturnsBadRequest()
    {
        // Arrange
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ReturnsAsync("data");
        _jsonErrorHandling.Setup(x => x.GeoJsonError("data")).Throws(new JsonException());
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_DbUpdateExceptionIsThrown_ReturnsBadRequest()
    {
        // Arrange
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ReturnsAsync("data");
        _jsonErrorHandling.Setup(x => x.GeoJsonError("data")).Returns(new OkResult());
        _jsonProcessorGeo.Setup(x => x.LongLatProcessor("data")).Returns(new CityData());
        _timeZoneApi.Setup(x => x.GetTimeZone(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("data");
        _jsonErrorHandling.Setup(x => x.TimeZoneJsonError("data")).Returns(new OkResult());
        _jsonProcessorTz.Setup(x => x.TimeZoneProcessor("data")).Returns(new CityData());
        _cityDataCombiner.Setup(x => x.CombineCityData(It.IsAny<CityData>(), It.IsAny<CityData>())).Returns(new CityData());
        _normalizeCityName.Setup(x => x.Normalize(City)).Returns(City);
        _geoRepository.Setup(x => x.AddCity(It.IsAny<CityData>())).Throws(new DbUpdateException());
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLocation_ExceptionIsThrown_ReturnsBadRequest()
    {
        // Arrange
        _geoRepository.Setup(x => x.GetCity(City)).Returns((CityData) null);
        _geoApi.Setup(x => x.GetLongLat(City)).ReturnsAsync("data");
        _jsonErrorHandling.Setup(x => x.GeoJsonError("data")).Returns(new OkResult());
        _jsonProcessorGeo.Setup(x => x.LongLatProcessor("data")).Returns(new CityData());
        _timeZoneApi.Setup(x => x.GetTimeZone(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("data");
        _jsonErrorHandling.Setup(x => x.TimeZoneJsonError("data")).Returns(new OkResult());
        _jsonProcessorTz.Setup(x => x.TimeZoneProcessor("data")).Returns(new CityData());
        _cityDataCombiner.Setup(x => x.CombineCityData(It.IsAny<CityData>(), It.IsAny<CityData>())).Returns(new CityData());
        _normalizeCityName.Setup(x => x.Normalize(City)).Returns(City);
        _geoRepository.Setup(x => x.AddCity(It.IsAny<CityData>())).Throws(new Exception());
        
        // Act
        var result = await _locationController.GetLocation(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}