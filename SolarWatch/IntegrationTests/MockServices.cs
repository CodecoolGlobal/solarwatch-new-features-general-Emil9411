using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using SolarWatch.Controllers;
using SolarWatch.Services.AuthServices;
using SolarWatch.Services.LocationServices;
using SolarWatch.Services.SwServices;
using SolarWatch.Services.UserServices;
using SolarWatch.Utilities;

namespace IntegrationTests;

internal class MockServices
{
    public Mock<ISwService> SwServiceMock { get; init; } = new();
    public Mock<ILocationService> LocationServiceMock { get; init; } = new();
    public Mock<IAuthService> AuthServiceMock { get; init; } = new();
    public Mock<IGeoRepository> GeoRepositoryMock { get; init; } = new();
    public Mock<ISwRepository> SwRepositoryMock { get; init; } = new();
    public Mock<INormalizeCityName> NormalizeCityNameMock { get; init; } = new();
    public Mock<IUserRepository> UserRepositoryMock { get; init; } = new();
    public Mock<ILogger<SwController>> SwLoggerMock { get; init; } = new();
    public Mock<ILogger<LocationController>> LocationLoggerMock { get; init; } = new();
    public Mock<ILogger<UserController>> UserLoggerMock { get; init; } = new();

    public IEnumerable<(Type, object)> GetMocks()
    {
        return GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p =>
            {
                var interfaceType = p.PropertyType.GetGenericArguments()[0];
                var value = p.GetValue(this) as Mock;
                return (interfaceType, value.Object);
            })
            .ToArray();
    }
}