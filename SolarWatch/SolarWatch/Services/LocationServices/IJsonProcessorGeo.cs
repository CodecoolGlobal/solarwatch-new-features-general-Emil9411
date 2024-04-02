using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public interface IJsonProcessorGeo
{
    CityData LongLatProcessor(string data);
}