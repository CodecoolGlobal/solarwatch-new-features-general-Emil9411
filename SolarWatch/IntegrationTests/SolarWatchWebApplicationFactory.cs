using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SolarWatch.Data;

namespace IntegrationTests;

internal class SolarWatchWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly MockServices _mockServices;
    
    public SolarWatchWebApplicationFactory(MockServices mockServices)
    {
        _mockServices = mockServices;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            foreach ((var interfaceType, var mock) in _mockServices.GetMocks())
            {
                services.Remove(services.SingleOrDefault(d => d.ServiceType == interfaceType));
                services.AddSingleton(interfaceType, mock);
            }
        });
        return base.CreateHost(builder);
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAdminScheme", options => { })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestUserScheme", options => { });
        });
    }
}