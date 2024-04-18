using System.Data.Common;
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
using SolarWatch.Services.SwServices;
using SolarWatch.Utilities;

namespace UnitTests;

public class LocationControllerTest
{
    private readonly Mock<ILogger<LocationController>> _logger = new();
    private readonly Mock<IGeoRepository> _geoRepository = new();
    private readonly Mock<ILocationService> _locationService = new();
    

    private readonly LocationController _locationController;

    public LocationControllerTest()
    {
        _locationController = new LocationController(_logger.Object, _locationService.Object, _geoRepository.Object);
    }
    
    private const string City = "New York";
    
    // get location endpoint tests
    [Test]
    public async Task GetLocation_WhenCityIsNullOrWhiteSpace_ReturnsBadRequest()
    {
        // Arrange
        var city = string.Empty;

        var badRequestResult = new BadRequestObjectResult("City is required");
        _locationService.Setup(x => x.GetLocation(city)).ReturnsAsync(badRequestResult);

        // Act
        var result = await _locationController.GetLocation(city);

        // Assert
        Assert.Multiple(() =>
        { 
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("City is required"));
        });
    }
    
    [Test]
    public async Task GetLocation_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _locationService.Setup(x => x.GetLocation(City)).ThrowsAsync(new Exception());

        // Act
        var result = await _locationController.GetLocation(City);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("Exception of type 'System.Exception' was thrown."));
        });
    }
    
    // get all cities endpoint tests
    [Test]
    public void GetAllCities_WhenCalled_ReturnsOk()
    {
        // Arrange
        var cities = new List<CityData>();
        _geoRepository.Setup(x => x.GetAllCities()).Returns(cities);

        // Act
        var result = _locationController.GetAllCities();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }
    
    [Test]
    public void GetAllCities_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _geoRepository.Setup(x => x.GetAllCities()).Throws(new Exception());

        // Act
        var result = _locationController.GetAllCities();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("Exception of type 'System.Exception' was thrown."));
        });
    }
    
    // update city endpoint tests
    [Test]
    public async Task Update_WhenCityDataIsNull_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updatedData = new CityData();
        var badRequestResult = new BadRequestObjectResult("City data is required");
        _locationService.Setup(x => x.UpdateLocation(id, updatedData)).ReturnsAsync(badRequestResult);

        // Act
        var result = await _locationController.Update(id, updatedData);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("City data is required"));
        });
    }
    
    [Test]
    public async Task Update_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updatedData = new CityData();
        _locationService.Setup(x => x.UpdateLocation(id, updatedData)).ThrowsAsync(new Exception());

        // Act
        var result = await _locationController.Update(id, updatedData);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("Exception of type 'System.Exception' was thrown."));
        });
    }
    
    [Test]
    public async Task Update_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updatedData = new CityData();
        _locationService.Setup(x => x.UpdateLocation(id, updatedData)).ThrowsAsync(new DbUpdateException());

        // Act
        var result = await _locationController.Update(id, updatedData);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result.Result).Value, Is.EqualTo("Exception of type 'Microsoft.EntityFrameworkCore.DbUpdateException' was thrown."));
        });
    }
    
    // delete city endpoint tests
    [Test]
    public async Task Delete_WhenCityDataIsNull_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        _geoRepository.Setup(x => x.GetCityById(id)).ReturnsAsync((CityData) null);

        // Act
        var result = await _locationController.Delete(id);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
    }
    
    [Test]
    public async Task Delete_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        _geoRepository.Setup(x => x.GetCityById(id)).ThrowsAsync(new Exception());

        // Act
        var result = await _locationController.Delete(id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Exception of type 'System.Exception' was thrown."));
        });
    }
    
    [Test]
    public async Task Delete_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var cityData = new CityData();
        _geoRepository.Setup(x => x.GetCityById(id)).ReturnsAsync(cityData);
        _geoRepository.Setup(x => x.DeleteCity(cityData)).Throws(new DbUpdateException());

        // Act
        var result = await _locationController.Delete(id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            Assert.That(((BadRequestObjectResult)result).Value, Is.EqualTo("Exception of type 'Microsoft.EntityFrameworkCore.DbUpdateException' was thrown."));
        });
    }
    
    [Test]
    public async Task Delete_WhenCityDataIsNotNull_ReturnsOk()
    {
        // Arrange
        var id = 1;
        var cityData = new CityData();
        _geoRepository.Setup(x => x.GetCityById(id)).ReturnsAsync(cityData);

        // Act
        var result = await _locationController.Delete(id);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}