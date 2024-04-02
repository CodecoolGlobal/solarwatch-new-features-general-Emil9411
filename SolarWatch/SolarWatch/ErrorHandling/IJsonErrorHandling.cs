using Microsoft.AspNetCore.Mvc;

namespace SolarWatch.ErrorHandling;

public interface IJsonErrorHandling
{
    ActionResult SolarJsonError(string solarJson);
    ActionResult GeoJsonError(string geoJson);
    ActionResult TimeZoneJsonError(string timeZoneJson);
}