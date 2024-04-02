using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SolarWatch.Model;

public class ApplicationUser : IdentityUser
{
    [Required]public string? City { get; set; }
}