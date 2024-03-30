using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Model;

public class ApplicationUser : IdentityUser
{
    public string City { get; set; }
}