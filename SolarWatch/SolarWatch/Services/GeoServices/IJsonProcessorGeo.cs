namespace SolarWatch.Services.GeoServices;

public interface IJsonProcessorGeo
{
    double[] LongLatProcessor(string data);
}