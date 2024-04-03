using System.Globalization;
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
using SolarWatch.Services.SWServices;
using SolarWatch.Utilities;

namespace UnitTests;

public class SolarWatchControllerTest
{
    private readonly Mock<ILogger<SwController>> _loggerMock = new();
    private readonly Mock<ISwApi> _swApiMock = new();
    private readonly Mock<IJsonProcessorSw> _jsonProcessorSwMock = new();
    private readonly Mock<IGeoApi> _geoApiMock = new();
    private readonly Mock<IJsonProcessorGeo> _jsonProcessorGeoMock = new();
    private readonly Mock<ISwRepository> _swRepositoryMock = new();
    private readonly Mock<IGeoRepository> _geoRepositoryMock = new();
    private readonly Mock<ITimeZoneApi> _timeZoneApiMock = new();
    private readonly Mock<IJsonProcessorTz> _jsonProcessorTzMock = new();
    private readonly Mock<IJsonErrorHandling> _jsonErrorHandlingMock = new();
    private readonly Mock<ICityDataCombiner> _cityDataCombinerMock = new();
    private readonly Mock<INormalizeCityName> _normalizeCityNameMock = new();


    private readonly SwController _swController;

    public SolarWatchControllerTest()
    {
        _swController = new SwController(_loggerMock.Object, _swApiMock.Object, _jsonProcessorSwMock.Object,
            _geoApiMock.Object, _jsonProcessorGeoMock.Object, _swRepositoryMock.Object, _geoRepositoryMock.Object,
            _jsonErrorHandlingMock.Object, _timeZoneApiMock.Object, _jsonProcessorTzMock.Object,
            _cityDataCombinerMock.Object, _normalizeCityNameMock.Object);
    }

    private const string City = "London";
    private static readonly DateOnly Date = new(2022, 1, 1);

    // get data endpoint tests
    [Test]
    public async Task GetData_WhenCityIsNullOrWhiteSpace_ReturnsBadRequest()
    {
        // Arrange
        var city = string.Empty;

        // Act
        var result = await _swController.GetData(city, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("City is required"));
        });
    }
    
    [Test]
    public async Task GetData_WhenDateIsDefault_ReturnsBadRequest()
    {
        // Arrange
        var date = default(DateOnly);

        // Act
        var result = await _swController.GetData(City, date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("Date is required"));
        });
    }
    
    [Test]
    public async Task GetData_WhenSwDataExistsInDb_ReturnsOk()
    {
        // Arrange
        var swData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns(swData);

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result.Result).Value, Is.EqualTo(swData));
        });
    }
    
    [Test]
    public async Task GetData_WhenGeoDataExistsInDb_ReturnsOk()
    {
        // Arrange
        var geoData = new CityData();
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns(geoData);

        var solarJson = "solarJson";
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        _swApiMock.Setup(x => x.GetSolarData(Date, geoData.Latitude, geoData.Longitude, geoData.TimeZone))
            .ReturnsAsync(solarJson);

        var solarData = new TimeOnly[2];
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);

        var newCity = new SwData();
        _swRepositoryMock.Setup(x => x.AddSwData(newCity));

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenGeoDataExistsInDbAndSolarJsonError_ReturnsError()
    {
        // Arrange
        var geoData = new CityData();
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns(geoData);

        var solarJson = "error";
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new BadRequestObjectResult("error"));
        _swApiMock.Setup(x => x.GetSolarData(Date, geoData.Latitude, geoData.Longitude, geoData.TimeZone))
            .ReturnsAsync(solarJson);

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenGeoDataDoesNotExistInDbAndGeoJsonError_ReturnsError()
    {
        // Arrange
        var geoData = "[]";
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync(geoData);
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError(geoData)).Returns(new NotFoundObjectResult("Data not found"));

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenGeoDataDoesNotExistInDbAndTimeZoneJsonError_ReturnsError()
    {
        // Arrange
        var geoData = "geoData";
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync(geoData);
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError(geoData)).Returns(new OkResult());

        var geoCityData = new CityData();
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor(geoData)).Returns(geoCityData);

        var latitude = geoCityData.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = geoCityData.Longitude.ToString(CultureInfo.InvariantCulture);
        
        var timeZoneJson = "error";
        _timeZoneApiMock.Setup(x => x.GetTimeZone(latitude, longitude)).ReturnsAsync(timeZoneJson);
        _jsonErrorHandlingMock.Setup(x => x.TimeZoneJsonError(timeZoneJson)).Returns(new BadRequestObjectResult("error"));

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetData_WhenGeoDataDoesNotExistInDbAndSolarJsonError_ReturnsBadRequest()
    {
        // Arrange
        var geoData = "geoData";
        _geoApiMock.Setup(x => x.GetLongLat(City)).ReturnsAsync(geoData);
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError(geoData)).Returns(new OkResult());

        var geoCityData = new CityData();
        _jsonProcessorGeoMock.Setup(x => x.LongLatProcessor(geoData)).Returns(geoCityData);

        var latitude = geoCityData.Latitude.ToString(CultureInfo.InvariantCulture);
        var longitude = geoCityData.Longitude.ToString(CultureInfo.InvariantCulture);
        
        var timeZoneJson = "timeZoneJson";
        _timeZoneApiMock.Setup(x => x.GetTimeZone(latitude, longitude)).ReturnsAsync(timeZoneJson);
        _jsonErrorHandlingMock.Setup(x => x.TimeZoneJsonError(timeZoneJson)).Returns(new OkResult());

        var timeZoneCityData = new CityData();
        _jsonProcessorTzMock.Setup(x => x.TimeZoneProcessor(timeZoneJson)).Returns(timeZoneCityData);

        var newCityData = new CityData();
        _cityDataCombinerMock.Setup(x => x.CombineCityData(geoCityData, timeZoneCityData)).Returns(newCityData);
        
        var normalizedCity = "normalizedCity";
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(normalizedCity);
        
        newCityData.City = normalizedCity;
        
        _geoRepositoryMock.Setup(x => x.AddCity(newCityData));
        
        var solarJson = "solarJson";
        _swApiMock.Setup(x => x.GetSolarData(Date, newCityData.Latitude, newCityData.Longitude, newCityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new BadRequestObjectResult("error"));

        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_HttpRequestException_ReturnsBadRequest()
    {
        // Arrange
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new HttpRequestException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetData_JsonException_ReturnsBadRequest()
    {
        // Arrange
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new JsonException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_DbUpdateException_ReturnsBadRequest()
    {
        // Arrange
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new DbUpdateException());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_UnknownException_ReturnsBadRequest()
    {
        // Arrange
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new Exception());
        
        // Act
        var result = await _swController.GetData(City, Date);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // get all data endpoint tests
    [Test]
    public void GetAll_ReturnsOk()
    {
        // Arrange
        var allData = new List<SwData>();
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Returns(allData);

        // Act
        var result = _swController.GetAll();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result.Result).Value, Is.EqualTo(allData));
        });
    }
    
    [Test]
    public void GetAll_DbUpdateException_ReturnsBadRequest()
    {
        // Arrange
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Throws(new DbUpdateException());

        // Act
        var result = _swController.GetAll();
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public void GetAll_UnknownException_ReturnsBadRequest()
    {
        // Arrange
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Throws(new Exception());

        // Act
        var result = _swController.GetAll();
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // update data endpoint tests
    [Test]
    public async Task Update_WhenSwDataDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        var updatedData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync((SwData) null);

        // Act
        var result = await _swController.Update(id, updatedData);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }
    
    [Test]
    public async Task Update_DbUpdateException_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updatedData = new SwData();
        var swData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.UpdateSwData(swData)).Throws(new DbUpdateException());

        // Act
        var result = await _swController.Update(id, updatedData);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Update_UnknownException_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updatedData = new SwData();
        var swData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.UpdateSwData(swData)).Throws(new Exception());

        // Act
        var result = await _swController.Update(id, updatedData);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // delete data endpoint tests
    [Test]
    public async Task Delete_WhenSwDataDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync((SwData) null);

        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    
    [Test]
    public async Task Delete_DbUpdateException_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new DbUpdateException());

        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Delete_UnknownException_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData();
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new Exception());

        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}