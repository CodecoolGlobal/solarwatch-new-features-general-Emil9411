using SolarWatch.Services.GeoServices;
using SolarWatch.Services.SWServices;
var builder = WebApplication.CreateBuilder(args);

AddServices();
ConfigureSwagger();
// AddDbContext();
// AddAuthentication();
// AddIdentity();

var app = builder.Build();

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
    builder.Services.AddSingleton<IGeoApi, GeoApi>();
    builder.Services.AddSingleton<IJsonProcessorGeo, JsonProcessorGeo>();
    builder.Services.AddSingleton<ISWApi, SWApi>();
    builder.Services.AddSingleton<IJsonProcessorSW, JsonProcessorSW>();
}

void ConfigureSwagger()
{
    builder.Services.AddSwaggerGen();
}