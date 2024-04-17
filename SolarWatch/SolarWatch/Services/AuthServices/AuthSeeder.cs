using Microsoft.AspNetCore.Identity;
using SolarWatch.Model;

namespace SolarWatch.Services.AuthServices;

public class AuthSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public AuthSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public void AddRoles()
    {
        var tAdmin = CreateAdminRole(_roleManager);
        tAdmin.Wait();
        
        var tUser = CreateUserRole(_roleManager);
        tUser.Wait();
    }
    
    public void AddAdmin()
    {
        var tAdmin = CreateAdminIfNotExists();
        tAdmin.Wait();
    }
    
    public void AddTestUser()
    {
        var tUser = CreateTestUserIfNotExists();
        tUser.Wait();
    }

    private async Task CreateAdminIfNotExists()
    {
        var adminInDb = await _userManager.FindByEmailAsync("admin@admin.com");
        if (adminInDb == null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@admin.com", City = "Admin"};
            var adminCreated = await _userManager.CreateAsync(admin, "admin123");

            if (adminCreated.Succeeded)
            {
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
    
    private async Task CreateTestUserIfNotExists()
    {
        var userInDb = await _userManager.FindByEmailAsync("test@test.com");
        if (userInDb == null)
        {
            var user = new ApplicationUser { UserName = "test", Email = "test@test.com", City = "Dublin"};
            var userCreated = await _userManager.CreateAsync(user, "test123");
            
            if (userCreated.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }
    }

    private async Task CreateAdminRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    private async Task CreateUserRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    } }