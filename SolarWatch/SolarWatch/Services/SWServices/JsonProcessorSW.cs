using System.Globalization;
using System.Text.Json;

namespace SolarWatch.Services.SWServices;

public class JsonProcessorSW : IJsonProcessorSW
{
    private static readonly string[] InputFormats = { "h:mm:ss tt", "hh:mm:ss tt" };
    private const string OutputFormat = "HH:mm:ss";

    public string[] SolarJsonProcessor(string data)
    {
        var returnSunrise = "";
        var returnSunset = "";

        var json = JsonDocument.Parse(data);

        var sunrise = json.RootElement.GetProperty("results").GetProperty("sunrise");
        var sunset = json.RootElement.GetProperty("results").GetProperty("sunset");

        var sunriseString = sunrise.GetString();
        var sunsetString = sunset.GetString();

        foreach (var format in InputFormats)
        {
            if (DateTime.TryParseExact(sunriseString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var sunriseTime))
            {
                returnSunrise = sunriseTime.ToString(OutputFormat);
            }

            if (DateTime.TryParseExact(sunsetString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var sunsetTime))
            {
                returnSunset = sunsetTime.ToString(OutputFormat);
            }
        }

        return new[] { returnSunrise, returnSunset };
    }
}