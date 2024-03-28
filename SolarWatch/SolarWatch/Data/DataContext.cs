using Microsoft.EntityFrameworkCore;
using SolarWatch.Model;
using SolarWatch.Utilities;

namespace SolarWatch.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<SWData>? SolarWatchDatas { get; set; }
    public DbSet<CityData>? CityDatas { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=SolarWatch;Data Source=EMIL\\SQLEXPRESS;TrustServerCertificate=true;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SWData>()
            .Property(e => e.Date)
            .HasConversion(new DateOnlyConverter());
        modelBuilder.Entity<SWData>()
            .Property(e => e.Sunrise)
            .HasConversion(new TimeOnlyConverter());
        modelBuilder.Entity<SWData>()
            .Property(e => e.Sunset)
            .HasConversion(new TimeOnlyConverter());
    }

}