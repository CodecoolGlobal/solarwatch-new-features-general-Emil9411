using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Services.UserServices;

public class UserRepository : IUserRepository
{
    private readonly UsersContext _context;
    private readonly IsValidEmail _isValidEmail = new();

    public UserRepository(UsersContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
    }
    
    public IEnumerable<UserResponse> GetAllUsers()
    {
        return _context.Users.Select(u => new UserResponse(u.Id, u.UserName, u.City));
    }
    
    public UserResponse GetUserByEmailOrUserName(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email || u.UserName == email);
        return new UserResponse(user.Id, user.UserName, user.City);
    }
    
    public ApplicationUser GetUserById(string id)
    {
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public ActionResult<UserResponse> UpdateUser(string id, UserResponse user)
    {
        var userToUpdate = _context.Users.FirstOrDefault(u => u.Id == id);
        if (userToUpdate == null)
        {
            return new BadRequestObjectResult("User not found");
        }
        
        userToUpdate.UserName = user.UserName;
        userToUpdate.City = user.City;
        _context.SaveChanges();
        return new UserResponse(userToUpdate.Id, userToUpdate.UserName, userToUpdate.City);
    }

    public void DeleteUser(string id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}