using System.Globalization;
using System.Text.Json;

namespace SolarWatch.Services.SWServices;

public class JsonProcessorSw : IJsonProcessorSw
{
    private static readonly string[] InputFormats = { "h:mm:ss tt", "hh:mm:ss tt" };

    public TimeOnly[] SolarJsonProcessor(string data)
    {
        TimeOnly returnSunrise = new();
        TimeOnly returnSunset = new();

        try
        {
            var json = JsonDocument.Parse(data);

            var rootResults = json.RootElement.GetProperty("results");
            var sunrise = rootResults.GetProperty("sunrise");
            var sunset = rootResults.GetProperty("sunset");

            var sunriseString = sunrise.GetString();
            var sunsetString = sunset.GetString();

            foreach (var format in InputFormats)
            {
                if (DateTime.TryParseExact(sunriseString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime sunriseTime))
                {
                    returnSunrise = new TimeOnly(sunriseTime.Hour, sunriseTime.Minute, sunriseTime.Second);
                }

                if (DateTime.TryParseExact(sunsetString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime sunsetTime))
                {
                    returnSunset = new TimeOnly(sunsetTime.Hour, sunsetTime.Minute, sunsetTime.Second);
                }
            }
        }
        catch (JsonException)
        {
            return new TimeOnly[] { };
        }
        catch (KeyNotFoundException)
        {
            return new TimeOnly[] { };
        }

        return new[] { returnSunrise, returnSunset };
    }
}