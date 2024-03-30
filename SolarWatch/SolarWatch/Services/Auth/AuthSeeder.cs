using Microsoft.AspNetCore.Identity;
using SolarWatch.Model;

namespace SolarWatch.Services.Auth;

public class AuthSeeder
{
    private RoleManager<IdentityRole> roleManager;
    private UserManager<ApplicationUser> userManager;
    
    public AuthSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
    }

    public void AddRoles()
    {
        var tAdmin = CreateAdminRole(roleManager);
        tAdmin.Wait();
        
        var tUser = CreateUserRole(roleManager);
        tUser.Wait();
    }
    
    public void AddAdmin()
    {
        var tAdmin = CreateAdminIfNotExists();
        tAdmin.Wait();
    }

    private async Task CreateAdminIfNotExists()
    {
        var adminInDb = await userManager.FindByEmailAsync("admin@admin.com");
        if (adminInDb == null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@admin.com", City = "Admin"};
            var adminCreated = await userManager.CreateAsync(admin, "admin123");

            if (adminCreated.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
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