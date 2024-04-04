using Microsoft.AspNetCore.Identity;
using SolarWatch.Data;
using SolarWatch.Model;

namespace SolarWatch.Services.UserServices;

public class UserRepository : IUserRepository
{
    private readonly UsersContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public UserRepository(UsersContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    
    public IEnumerable<UserResponse> GetAllUsers()
    {
        return _context.Users.Select(u => new UserResponse(u.Email, u.UserName, u.City, u.PhoneNumber));
    }
    
    public UserResponse GetUserByEmailOrUserName(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email || u.UserName == email);
        return new UserResponse(user.Email, user.UserName, user.City, user.PhoneNumber);
    }
    
    public ApplicationUser GetUserById(string id)
    {
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public UserResponse UpdateUser(string id, UserResponse user)
    {
        var userFromDb = _context.Users.FirstOrDefault(u => u.Id == id);
        if (userFromDb != null)
        {
            userFromDb.Email = user.Email;
            userFromDb.UserName = user.UserName;
            userFromDb.City = user.City;
            userFromDb.PhoneNumber = user.PhoneNumber;
            _context.Users.Update(userFromDb);
            _context.SaveChanges();
        }
        return new UserResponse(userFromDb.Email, userFromDb.UserName, userFromDb.City, userFromDb.PhoneNumber);
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