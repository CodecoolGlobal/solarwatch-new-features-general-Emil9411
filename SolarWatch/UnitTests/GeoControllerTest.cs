using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SolarWatch.Controllers;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.GeoServices;

namespace UnitTests;

public class GeoControllerTest
{
    private readonly Mock<ILogger<GeoController>> _loggerMock = new();
    private readonly Mock<IGeoApi> _geoApiMock = new();
    private readonly Mock<IJsonProcessorGeo> _jsonProcessorGeoMock = new();
    private readonly Mock<IGeoRepository> _geoRepositoryMock = new();
    private readonly Mock<IJsonErrorHandling> _jsonErrorHandlingMock = new();
    
    private readonly GeoController _geoController;
    
    public GeoControllerTest()
    {
        _geoController = new GeoController(_loggerMock.Object, _geoApiMock.Object, _jsonProcessorGeoMock.Object, _geoRepositoryMock.Object, _jsonErrorHandlingMock.Object);
    }

    private const string City = "London";
    
    [Test]
    public async Task GetLongLat_WhenCityExistsInDb_ReturnsOk()
    {
        // Arrange
        var cityData = new CityData
        {
            City = City,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(City); 
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
        });
    }

    [Test]
    public async Task GetLongLat_WhenCityDoesNotExistInDb_ReturnsOk()
    {
        // Arrange
        var cityData = new CityData
        {
            City = City,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("json");
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError("json")).Returns(new OkResult());
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
        });
    }

    [Test]
    public async Task GetLongLat_WhenApiCallFails_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new HttpRequestException());
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenJsonProcessingFails_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new JsonException());
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenUnknownErrorOccurs_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new Exception());
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
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
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("[]");
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError("[]")).Returns(new NotFoundObjectResult("Data not found"));
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetLongLat_WhenCityNotFoundInDb_ReturnsOk()
    {
        // Arrange
        var cityData = new CityData
        {
            City = City,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("json");
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError("json")).Returns(new OkResult());
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Returns(cityData);
        
        // Act
        var result = await _geoController.GetLongLat(City);
        Assert.Multiple(() =>
        {

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(cityData));
        });
    }

    [Test]
    public async Task GetLongLat_WhenJsonExceptionOccurs_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync("json");
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new JsonException());
        
        // Act
        var result = await _geoController.GetLongLat(City);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}