using System.Text.Json;
using SolarWatch.Model;

namespace SolarWatch.Services.LocationServices;

public class JsonProcessorTz : IJsonProcessorTz
{
    public CityData TimeZoneProcessor(string data)
    {
        var json = JsonDocument.Parse(data);
        var results = json.RootElement;
        var zone = results.GetProperty("zoneName").GetString();
        var country = results.GetProperty("countryName").GetString();
        return new CityData
        {
            TimeZone = zone,
            Country = country
        };
    }
}