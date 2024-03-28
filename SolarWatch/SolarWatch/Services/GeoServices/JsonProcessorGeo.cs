using System.Text.Json;

namespace SolarWatch.Services.GeoServices;

public class JsonProcessorGeo : IJsonProcessorGeo
{
    public double[] LongLatProcessor(string data)
    {
        var json = JsonDocument.Parse(data);
        var results = json.RootElement[0];
        var lat = results.GetProperty("lat").GetDouble();
        var lon = results.GetProperty("lon").GetDouble();
        return new double[] {lat, lon};
    }
}