using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SolarWatch.Controllers;
using SolarWatch.Model;
using SolarWatch.Services.UserServices;

namespace UnitTests;

public class UserControllerTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private UserController _userController;

    [SetUp]
    public void Setup()
    {
        _userController = new UserController(_userRepositoryMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void GetAllUsers_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAllUsers()).Returns(new List<UserResponse>());

        // Act
        var result = _userController.GetAllUsers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void GetAllUsers_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetAllUsers()).Throws(new Exception());

        // Act
        var result = _userController.GetAllUsers();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetUserByEmailOrUserName_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        var mockUserResponse = new UserResponse("", "", "");
        _userRepositoryMock.Setup(x => x.GetUserByEmailOrUserName(It.IsAny<string>())).Returns(mockUserResponse);

        // Act
        var result = _userController.GetUserByEmailOrUserName("test");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void GetUserByEmailOrUserName_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetUserByEmailOrUserName(It.IsAny<string>())).Throws(new Exception());

        // Act
        var result = _userController.GetUserByEmailOrUserName("test");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetUserById_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetUserById(It.IsAny<string>())).Returns(new ApplicationUser());

        // Act
        var result = _userController.GetUserById("test");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void GetUserById_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetUserById(It.IsAny<string>())).Throws(new Exception());

        // Act
        var result = _userController.GetUserById("test");

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void UpdateUser_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        var id = "1";
        var mockUser = new UserResponse("", "", "");
        var mockUserResponse = new UserResponse("1", "Test", "Test");
        _userRepositoryMock.Setup(x => x.UpdateUser(id, mockUser)).Returns(mockUserResponse);
        
        // Act
        var result = _userController.UpdateUser(id, mockUser);
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public void UpdateUser_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.UpdateUser(It.IsAny<string>(), It.IsAny<UserResponse>())).Throws(new Exception());
        
        // Act
        var result = _userController.UpdateUser("1", new UserResponse("", "", ""));
        
        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void DeleteUser_WhenCalled_ReturnsOkResult()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.DeleteUser(It.IsAny<string>()));

        // Act
        var result = _userController.DeleteUser("test");

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public void DeleteUser_WhenExceptionThrown_ReturnsBadRequest()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.DeleteUser(It.IsAny<string>())).Throws(new Exception());

        // Act
        var result = _userController.DeleteUser("test");

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}