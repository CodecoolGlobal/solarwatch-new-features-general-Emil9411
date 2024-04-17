using System.Globalization;
using System.Text.Json;

namespace SolarWatch.Services.SwServices;

public class JsonProcessorSw : IJsonProcessorSw
{
    private static readonly string[] InputFormats = { "h:mm:ss tt", "hh:mm:ss tt" };

    public TimeOnly[] SolarJsonProcessor(string data)
    {
        TimeOnly returnSunrise = new();
        TimeOnly returnSunset = new();
        TimeOnly returnSolarNoon = new();

        try
        {
            var json = JsonDocument.Parse(data);

            var rootResults = json.RootElement.GetProperty("results");
            var sunrise = rootResults.GetProperty("sunrise");
            var sunset = rootResults.GetProperty("sunset");
            var solarNoon = rootResults.GetProperty("solar_noon");

            var sunriseString = sunrise.GetString();
            var sunsetString = sunset.GetString();
            var solarNoonString = solarNoon.GetString();

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

                if (DateTime.TryParseExact(solarNoonString, format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime solarNoonTime))
                {
                    returnSolarNoon = new TimeOnly(solarNoonTime.Hour, solarNoonTime.Minute, solarNoonTime.Second);
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

        return new[] { returnSunrise, returnSunset, returnSolarNoon};
    }

    public string DayLengthJsonProcessor(string data)
    {
        try
        {
            var json = JsonDocument.Parse(data);

            var rootResults = json.RootElement.GetProperty("results");
            var dayLength = rootResults.GetProperty("day_length");

            var dayLengthString = dayLength.GetString();

            return dayLengthString;
        }
        catch (JsonException)
        {
            return "0";
        }
        catch (KeyNotFoundException)
        {
            return "0";
        }
    }
}