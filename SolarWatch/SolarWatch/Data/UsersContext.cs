using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SolarWatch.Model;

namespace SolarWatch.Data;

public class UsersContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    private readonly IConfiguration _config;
    public UsersContext(DbContextOptions<UsersContext> options, IConfiguration config) : base(options)
    {
        _config = config;
    }
    
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     var connectionString = _config.GetConnectionString("DatabaseConnection");
    //     Console.WriteLine(connectionString);
    //     optionsBuilder.UseSqlServer(connectionString);
    // }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}