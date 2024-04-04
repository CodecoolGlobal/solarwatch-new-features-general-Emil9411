using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolarWatch.Data;
using SolarWatch.ErrorHandling;
using SolarWatch.Model;
using SolarWatch.Services.AuthServices;
using SolarWatch.Services.LocationServices;
using SolarWatch.Services.SwServices;
using SolarWatch.Services.UserServices;
using SolarWatch.Utilities;

var builder = WebApplication.CreateBuilder(args);

AddServices();
ConfigureSwagger();
AddDbContext();
AddAuthentication();
AddIdentity();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var authenticationSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
authenticationSeeder.AddRoles();
authenticationSeeder.AddAdmin();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
return;

void AddServices()
{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllers(
        options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
    builder.Services.AddScoped<IGeoRepository, GeoRepository>();
    builder.Services.AddScoped<ISwRepository, SwRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ICityDataCombiner, CityDataCombiner>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<AuthSeeder>();
    builder.Services.AddSingleton<INormalizeCityName, NormalizeCityName>();
    builder.Services.AddSingleton<SpecialCharReplacements>();
    builder.Services.AddSingleton<IJsonErrorHandling, JsonErrorHandling>();
    builder.Services.AddSingleton<IGeoApi, GeoApi>();
    builder.Services.AddSingleton<IJsonProcessorGeo, JsonProcessorGeo>();
    builder.Services.AddSingleton<ISwApi, SwApi>();
    builder.Services.AddSingleton<IJsonProcessorSw, JsonProcessorSw>();
    builder.Services.AddSingleton<ITimeZoneApi, TimeZoneApi>();
    builder.Services.AddSingleton<IJsonProcessorTz, JsonProcessorTz>();
    
}

void ConfigureSwagger()
{
    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });
}

void AddDbContext()
{
    builder.Services.AddDbContext<DataContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection"));
    });
    builder.Services.AddDbContext<UsersContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection"));
    });
}

void AddIdentity()
{
    builder.Services
        .AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<UsersContext>();
}

void AddAuthentication()
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddCookie(options => {
            options.Cookie.Name = "Authorization";
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            var issuerSigningKey = builder.Configuration.GetSection("IssuerSigningKey").Value;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.ValidIssuer,
                ValidAudience = jwtSettings.ValidAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Cookies.ContainsKey("Authorization"))
                    {
                        context.Token = context.Request.Cookies["Authorization"];
                    }
                    return Task.CompletedTask;
                }
            };
        });
}