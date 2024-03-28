using SolarWatch.Model;

namespace SolarWatch.Services.GeoServices;

public interface IJsonProcessorGeo
{
    CityData LongLatProcessor(string data);
}