using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SolarWatch.Data;

namespace IntegrationTests;

internal class SolarWatchWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        });
        
        builder.ConfigureServices((context, services) =>
        {
            services.RemoveAll(typeof(DbContextOptions<DataContext>));
            services.RemoveAll(typeof(DbContextOptions<UsersContext>));
            
            var configuration = context.Configuration;
            var connectionString = configuration.GetConnectionString("DatabaseConnection");

            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddDbContext<UsersContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            
            services.AddAuthentication("TestUserScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestUserScheme", options => { });
            
            services.AddAuthentication("TestAdminScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAdminScheme", options => { });

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var usersContext = scope.ServiceProvider.GetRequiredService<UsersContext>();

                if (!dataContext.Database.CanConnect() || !usersContext.Database.CanConnect())
                {
                    
                    dataContext.Database.EnsureCreated();
                    usersContext.Database.Migrate(); 
                    usersContext.Database.EnsureCreated();
                }
                else
                {
                    dataContext.Database.EnsureDeleted();
                    usersContext.Database.EnsureDeleted();
        
                    // Recreate the database
                    dataContext.Database.EnsureCreated();
                    usersContext.Database.Migrate();
                    usersContext.Database.EnsureCreated();
                }
            }
        });
    }
}