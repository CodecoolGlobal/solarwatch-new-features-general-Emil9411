using Microsoft.AspNetCore.Identity;
using SolarWatch.Model;

namespace SolarWatch.Services.AuthServices;

public class AuthSeeder
{
    private RoleManager<IdentityRole> _roleManager;
    private UserManager<ApplicationUser> _userManager;
    
    public AuthSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        this._roleManager = roleManager;
        this._userManager = userManager;
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

    private async Task CreateAdminRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    private async Task CreateUserRole(RoleManager<IdentityRole> roleManager)
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }
}