using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SolarWatch.Controllers;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.GeoServices;
using SolarWatch.Services.SWServices;

namespace UnitTests;

public class SolarWatchControllerTest
{
    private readonly Mock<ILogger<SWController>> _loggerMock = new();
    private readonly Mock<ISWApi> _swApiMock = new();
    private readonly Mock<IJsonProcessorSW> _jsonProcessorSWMock = new();
    private readonly Mock<IGeoApi> _geoApiMock = new();
    private readonly Mock<IJsonProcessorGeo> _jsonProcessorGeoMock = new();
    private readonly Mock<ISWRepository> _swRepositoryMock = new();
    private readonly Mock<IGeoRepository> _geoRepositoryMock = new();
    private readonly Mock<IJsonErrorHandling> _jsonErrorHandlingMock = new();
    
    private readonly SWController _swController;
    
    public SolarWatchControllerTest()
    {
        _swController = new SWController(_loggerMock.Object, _swApiMock.Object, _jsonProcessorSWMock.Object, _geoApiMock.Object, _jsonProcessorGeoMock.Object, _swRepositoryMock.Object, _geoRepositoryMock.Object, _jsonErrorHandlingMock.Object);
    }
    
    private const string City = "London";
    private static readonly DateOnly Date = new(2022, 1, 1);

    [Test]
    public async Task GetData_WhenCityAndDateAreValid_ReturnsOk()
    {
        // Arrange
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };
        
        _swRepositoryMock.Setup(x => x.GetSWData(City, Date)).Returns(swData);
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        // Act
        var result = await _swController.GetData("", Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenDateIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        // Act
        var result = await _swController.GetData(City, default);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDb_ReturnsOk()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        
        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);
        
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDbAndApiCallFails_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new HttpRequestException());
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError(It.IsAny<string>())).Returns(new BadRequestObjectResult("error"));
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDbAndApiCallSucceedsButJsonError_ReturnsBadRequest()
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
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor("json")).Throws(new JsonException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDbAndApiCallSucceedsAndJsonError_ReturnsBadRequest()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Throws(new JsonException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDbAndApiCallSucceeds_ReturnsOk()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        
        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);
        
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsInDb_ReturnsOk()
    {
        // Arrange
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };
        
        _swRepositoryMock.Setup(x => x.GetSWData(City, Date)).Returns(swData);
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsInDbButJsonError_ReturnsBadRequest()
    {
        // Arrange
        var cityData = new CityData
        {
            City = City,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns(cityData);
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new BadRequestObjectResult("error"));
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsInDbAndJsonProcessWorksAndDataIsAddedToDb_ReturnsOk()
    {
        // Arrange
        var cityData = new CityData
        {
            City = City,
            Latitude = 51.5074,
            Longitude = 0.1278
        };
        
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns(cityData);
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        
        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);
        
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };
        
        _swRepositoryMock.Setup(x => x.GetSWData(City, Date)).Returns((SWData)null);
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsNotInDbAndApiCallSucceedsAndJsonProcessWorksAndDataIsAddedToDb_ReturnsOk()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        
        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);
        
        var swData = new SWData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };
        
        _swRepositoryMock.Setup(x => x.GetSWData(City, Date)).Returns((SWData)null);
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsNotInDbAndApiCallSucceedsButJsonError_ReturnsBadRequest()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new BadRequestObjectResult("error"));
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_DbUpdateException_ReturnsBadRequest()
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
        
        var solarJson = "json";
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude)).ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        
        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSWMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);
        
        _swRepositoryMock.Setup(x => x.GetSWData(City, Date)).Returns((SWData)null);
        _swRepositoryMock.Setup(x => x.AddSWData(It.IsAny<SWData>())).Throws(new DbUpdateException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    
    
    

}