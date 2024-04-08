using Microsoft.EntityFrameworkCore;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Data;

public class DataContext : DbContext
{
    private readonly IConfiguration _config;
    public DataContext(DbContextOptions<DataContext> options, IConfiguration config) : base(options)
    {
        _config = config;
    }

    public DbSet<SwData>? SolarWatchDataTable { get; set; }
    public DbSet<CityData>? CityDataTable { get; set; }
    
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     var connectionString = _config.GetConnectionString("DatabaseConnection");
    //     optionsBuilder.UseSqlServer(connectionString);
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SwData>()
            .Property(e => e.Date)
            .HasConversion(new DateOnlyConverter());
        modelBuilder.Entity<SwData>()
            .Property(e => e.Sunrise)
            .HasConversion(new TimeOnlyConverter());
        modelBuilder.Entity<SwData>()
            .Property(e => e.Sunset)
            .HasConversion(new TimeOnlyConverter());
    }

}