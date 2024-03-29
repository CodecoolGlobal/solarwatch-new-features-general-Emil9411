using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SolarWatch.Controllers;
using SolarWatch.Model;
using SolarWatch.Services.GeoServices;

namespace UnitTests;

public class GeoControllerTest
{
    private readonly Mock<ILogger<GeoController>> _loggerMock = new();
    private readonly Mock<IGeoApi> _geoApiMock = new();
    private readonly Mock<IJsonProcessorGeo> _jsonProcessorGeoMock = new();
    private readonly Mock<IGeoRepository> _geoRepositoryMock = new();
    
    private readonly GeoController _geoController;
    
    public GeoControllerTest()
    {
        _geoController = new GeoController(_loggerMock.Object, _geoApiMock.Object, _jsonProcessorGeoMock.Object, _geoRepositoryMock.Object);
    }
    
    [Test]
    public async Task GetLongLat_WhenCityExistsInDb_ReturnsOk()
    {
        // Arrange
        var city = "London";
        var cityData = new CityData
        {
            City = city,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
    }
    
    [Test]
    public async Task GetLongLat_WhenCityDoesNotExistInDb_ReturnsOk()
    {
        // Arrange
        var city = "London";
        var cityData = new CityData
        {
            City = city,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
    }
    
    [Test]
    public async Task GetLongLat_WhenApiCallFails_ReturnsBadRequest()
    {
        // Arrange
        var city = "London";
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ThrowsAsync(new HttpRequestException());
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenJsonProcessingFails_ReturnsBadRequest()
    {
        // Arrange
        var city = "London";
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new JsonException());
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenUnknownErrorOccurs_ReturnsBadRequest()
    {
        // Arrange
        var city = "London";
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new Exception());
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var city = "";
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityIsNull_ReturnsBadRequest()
    {
        // Arrange
        string city = null;
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityIsWhitespace_ReturnsBadRequest()
    {
        // Arrange
        var city = " ";
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityNotFound_ReturnsNotFound()
    {
        // Arrange
        var city = "London";
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("[]");
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityNotFoundInDb_ReturnsOk()
    {
        // Arrange
        var city = "London";
        var cityData = new CityData
        {
            City = city,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
    }
    
    [Test]
    public async Task GetLongLat_WhenJsonExceptionOccurs_ReturnsBadRequest()
    {
        // Arrange
        var city = "London";
        
        _geoRepositoryMock.Setup(x => x.GetCity(city)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(city)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new JsonException());
        
        // Act
        var result = await _geoController.GetLongLat(city);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}