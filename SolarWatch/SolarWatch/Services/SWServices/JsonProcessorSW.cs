using System.Globalization;
using System.Text.Json;

namespace SolarWatch.Services.SWServices;

public class JsonProcessorSW : IJsonProcessorSW
{
    private static readonly string[] InputFormats = { "h:mm:ss tt", "hh:mm:ss tt" };
    private const string OutputFormat = "HH:mm:ss";

    public TimeOnly[] SolarJsonProcessor(string data)
    {
        TimeOnly returnSunrise = new();
        TimeOnly returnSunset = new();

        var json = JsonDocument.Parse(data);

        var sunrise = json.RootElement.GetProperty("results").GetProperty("sunrise");
        var sunset = json.RootElement.GetProperty("results").GetProperty("sunset");

        var sunriseString = sunrise.GetString();
        var sunsetString = sunset.GetString();

        foreach (var format in InputFormats)
        {
            if (DateTime.TryParseExact(sunriseString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sunriseTime))
            {
                returnSunrise = new TimeOnly(sunriseTime.Hour, sunriseTime.Minute, sunriseTime.Second);
            }

            if (DateTime.TryParseExact(sunsetString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sunsetTime))
            {
                returnSunset = new TimeOnly(sunsetTime.Hour, sunsetTime.Minute, sunsetTime.Second);
            }
        }

        return new[] { returnSunrise, returnSunset };
    }
}