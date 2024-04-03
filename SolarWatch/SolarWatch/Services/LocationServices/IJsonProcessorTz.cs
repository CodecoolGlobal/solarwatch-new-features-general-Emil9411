using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public interface IJsonProcessorTz
{
    CityData TimeZoneProcessor(string data);
}