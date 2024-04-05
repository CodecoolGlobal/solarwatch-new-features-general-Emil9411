using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;
using SolarWatch.Services.UserServices;

namespace SolarWatch.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    
    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    [HttpGet("getall"), Authorize(Roles = "Admin")]
    public ActionResult<IEnumerable<UserResponse>> GetAllUsers()
    {
        try
        {
            return Ok(_userRepository.GetAllUsers());
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("getbyuserdata/{emailOrUserName}"), Authorize(Roles = "User,Admin")]
    public ActionResult<UserResponse> GetUserByEmailOrUserName([Required] string emailOrUserName)
    {
        try
        {
            return Ok(_userRepository.GetUserByEmailOrUserName(emailOrUserName));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("getbyid/{id}"), Authorize(Roles = "Admin")]
    public ActionResult<ApplicationUser> GetUserById([Required] string id)
    {
        try
        {
            return Ok(_userRepository.GetUserById(id));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPatch("update/{id}"), Authorize(Roles = "User,Admin")]
    public ActionResult<UserResponse> UpdateUser([Required] string id, [FromBody] UserResponse user)
    {
        try
        {
            var updatedUser = _userRepository.UpdateUser(id, user);
            return Ok(updatedUser);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpDelete("delete/{id}"), Authorize(Roles = "User, Admin")]
    public ActionResult DeleteUser([Required] string id)
    {
        try
        {
            _userRepository.DeleteUser(id);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}