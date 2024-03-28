using System.Text.Json;
using SolarWatch.Model;

namespace SolarWatch.Services.GeoServices;

public class JsonProcessorGeo : IJsonProcessorGeo
{
    public CityData LongLatProcessor(string data)
    {
        var json = JsonDocument.Parse(data);
        var results = json.RootElement[0];
        var lat = results.GetProperty("lat").GetDouble();
        var lon = results.GetProperty("lon").GetDouble();
        var city = results.GetProperty("name").GetString();
        return new CityData
        {
            City = city,
            Latitude = lat,
            Longitude = lon
        };
    }
}