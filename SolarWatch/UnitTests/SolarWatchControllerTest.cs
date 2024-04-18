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
using SolarWatch.Services.SwServices;
using SolarWatch.Utilities;

namespace UnitTests;

public class SolarWatchControllerTest
{
    private readonly Mock<ILogger<SwController>> _loggerMock = new();
    private readonly Mock<ISwRepository> _swRepositoryMock = new();
    private readonly Mock<INormalizeCityName> _normalizeCityNameMock = new();
    private readonly Mock<ISwService> _swServiceMock = new();


    private readonly SwController _swController;

    public SolarWatchControllerTest()
    {
        _swController = new SwController(_loggerMock.Object, _swRepositoryMock.Object, _normalizeCityNameMock.Object,
            _swServiceMock.Object);
    }

    private const string City = "London";
    private static readonly DateOnly Date = new(2022, 1, 1);

    // getdata endpoint tests
    [Test]
    public async Task GetData_WhenCityAndDateProvided_ReturnsSwData()
    {
        // Arrange
        var normalizedCity = "london";
        var swData = new SwData();
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(normalizedCity);
        _swServiceMock.Setup(x => x.GetSwData(normalizedCity, Date)).ReturnsAsync(swData);

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result, Is.InstanceOf<ActionResult<SwData>>());
        Assert.That(result.Value, Is.EqualTo(swData));
    }
    
    [Test]
    public async Task GetData_WhenHttpRequestExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(City);
        _swServiceMock.Setup(x => x.GetSwData(City, Date)).ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenJsonExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(City);
        _swServiceMock.Setup(x => x.GetSwData(City, Date)).ThrowsAsync(new JsonException());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(City);
        _swServiceMock.Setup(x => x.GetSwData(City, Date)).ThrowsAsync(new DbUpdateException());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetData_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _normalizeCityNameMock.Setup(x => x.Normalize(City)).Returns(City);
        _swServiceMock.Setup(x => x.GetSwData(City, Date)).ThrowsAsync(new Exception());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // getall endpoint tests
    [Test]
    public void GetAll_WhenCalled_ReturnsAllSwData()
    {
        // Arrange
        var allData = new List<SwData>();
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Returns(allData);

        // Act
        var result = _swController.GetAll();

        // Assert
        Assert.That(result.Value, Is.EqualTo(null));
    }
    
    [Test]
    public void GetAll_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Throws(new DbUpdateException());

        // Act
        var result = _swController.GetAll();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public void GetAll_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Throws(new Exception());

        // Act
        var result = _swController.GetAll();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // update endpoint tests
    [Test]
    public async Task Update_WhenCalled_ReturnsUpdatedSwData()
    {
        // Arrange
        var id = 1;
        var swData = new SwData();
        _swServiceMock.Setup(x => x.UpdateSwData(id, swData)).ReturnsAsync(swData);
        
        // Act
        var result = await _swController.Update(id, swData);
        
        // Assert
        Assert.That(result.Value, Is.EqualTo(swData));
    }
    
    [Test]
    public async Task Update_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData();
        _swServiceMock.Setup(x => x.UpdateSwData(id, swData)).ThrowsAsync(new DbUpdateException());
        
        // Act
        var result = await _swController.Update(id, swData);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Update_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData();
        _swServiceMock.Setup(x => x.UpdateSwData(id, swData)).ThrowsAsync(new Exception());
        
        // Act
        var result = await _swController.Update(id, swData);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    // delete endpoint tests
    [Test]
    public async Task Delete_WhenCalled_ReturnsOk()
    {
        // Arrange
        var id = 1;
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(new SwData());
        
        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
    
    [Test]
    public async Task Delete_WhenDbUpdateExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(new SwData());
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new DbUpdateException());
        
        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task Delete_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(new SwData());
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new Exception());
        
        // Act
        var result = await _swController.Delete(id);
        
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}