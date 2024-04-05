using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;

namespace SolarWatch.Services.UserServices;

public interface IUserRepository
{
    IEnumerable<UserResponse> GetAllUsers();
    UserResponse GetUserByEmailOrUserName(string email);
    ApplicationUser GetUserById(string id);
    ActionResult<UserResponse> UpdateUser(string id, UserResponse user);
    void DeleteUser(string id);
}