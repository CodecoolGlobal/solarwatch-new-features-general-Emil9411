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
    public async Task GetData_WhenCityAndDateAreValid_ReturnsOk()
    {
        // Arrange
        var swData = new SwData
        {
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns(swData);

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
    public async Task GetData_WhenCityAndDateAreValidButCityIsNotInDbAndApiCallFails_ReturnsBadRequest()
    {
        // Arrange
        _geoRepositoryMock.Setup(x => x.GetCity(City)).Returns((CityData)null);
        _geoApiMock.Setup(x => x.GetLongLat(City)).ThrowsAsync(new HttpRequestException());
        _jsonErrorHandlingMock.Setup(x => x.GeoJsonError(It.IsAny<string>()))
            .Returns(new BadRequestObjectResult("error"));

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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Throws(new JsonException());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetData_WhenCityAndDateAreValidAndCityIsInDb_ReturnsOk()
    {
        // Arrange
        var swData = new SwData
        {
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns(swData);

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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());

        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);

        var swData = new SwData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns((SwData)null);

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
    public async Task
        GetData_WhenCityAndDateAreValidAndCityIsNotInDbAndApiCallSucceedsAndJsonProcessWorksAndDataIsAddedToDb_ReturnsOk()
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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());

        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);

        var swData = new SwData
        {
            City = City,
            Date = Date,
            Sunrise = solarData[0],
            Sunset = solarData[1]
        };

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns((SwData)null);

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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());

        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns((SwData)null);
        _swRepositoryMock.Setup(x => x.AddSwData(It.IsAny<SwData>())).Throws(new DbUpdateException());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetData_Exception_ReturnsBadRequest()
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
        _swApiMock.Setup(x => x.GetSolarData(Date, cityData.Latitude, cityData.Longitude, cityData.TimeZone))
            .ReturnsAsync(solarJson);
        _jsonErrorHandlingMock.Setup(x => x.SolarJsonError(solarJson)).Returns(new OkResult());

        var solarData = new[] { new TimeOnly(8, 0), new TimeOnly(16, 0) };
        _jsonProcessorSwMock.Setup(x => x.SolarJsonProcessor(solarJson)).Returns(solarData);

        _swRepositoryMock.Setup(x => x.GetSwData(City, Date)).Returns((SwData)null);
        _swRepositoryMock.Setup(x => x.AddSwData(It.IsAny<SwData>())).Throws(new Exception());

        // Act
        var result = await _swController.GetData(City, Date);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // get all endpoint tests
    [Test]
    public void GetAll_ReturnsOk()
    {
        // Arrange
        var swData = new List<SwData>
        {
            new SwData
            {
                City = City,
                Date = Date,
                Sunrise = new TimeOnly(8, 0),
                Sunset = new TimeOnly(16, 0)
            }
        };

        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Returns(swData);

        // Act
        var result = _swController.GetAll();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result.Result as OkObjectResult)?.Value, Is.EqualTo(swData));
        });
    }

    [Test]
    public void GetAll_DbException_ReturnsBadRequest()
    {
        // Arrange
        _swRepositoryMock.Setup(x => x.GetAllSwDatas()).Throws(new DbUpdateException());

        // Act
        var result = _swController.GetAll();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetAll_Exception_ReturnsBadRequest()
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
    public async Task Update_WhenIdIsValid_ReturnsOk()
    {
        // Arrange
        var id = 1;
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);

        var newSwData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(9, 0),
            Sunset = new TimeOnly(17, 0)
        };

        // Act
        var result = await _swController.Update(id, newSwData);

        // Assert
        Assert.Multiple(() => { Assert.That(result.Result, Is.InstanceOf<OkObjectResult>()); });
    }

    [Test]
    public async Task Update_WhenSwDataIsNull_ReturnsNotFound()
    {
        // Arrange
        var id = 1;

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync((SwData)null);

        var newSwData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(9, 0),
            Sunset = new TimeOnly(17, 0)
        };

        // Act
        var result = await _swController.Update(id, newSwData);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_DbUpdateException_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);

        var newSwData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(9, 0),
            Sunset = new TimeOnly(17, 0)
        };

        _swRepositoryMock.Setup(x => x.UpdateSwData(newSwData)).Throws(new DbUpdateException());

        // Act
        var result = await _swController.Update(id, newSwData);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_Exception_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);

        var newSwData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(9, 0),
            Sunset = new TimeOnly(17, 0)
        };

        _swRepositoryMock.Setup(x => x.UpdateSwData(newSwData)).Throws(new Exception());

        // Act
        var result = await _swController.Update(id, newSwData);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    // delete endpoint tests
    [Test]
    public async Task Delete_WhenIdIsValid_ReturnsOk()
    {
        // Arrange
        var id = 1;
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);

        // Act
        var result = await _swController.Delete(id);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Delete_WhenSwDataIsNull_ReturnsNotFound()
    {
        // Arrange
        var id = 1;

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync((SwData)null);

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
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new DbUpdateException());

        // Act
        var result = await _swController.Delete(id);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Delete_Exception_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var swData = new SwData
        {
            Id = id,
            City = City,
            Date = Date,
            Sunrise = new TimeOnly(8, 0),
            Sunset = new TimeOnly(16, 0)
        };

        _swRepositoryMock.Setup(x => x.GetSwDataById(id)).ReturnsAsync(swData);
        _swRepositoryMock.Setup(x => x.DeleteSwData(id)).Throws(new Exception());

        // Act
        var result = await _swController.Delete(id);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}