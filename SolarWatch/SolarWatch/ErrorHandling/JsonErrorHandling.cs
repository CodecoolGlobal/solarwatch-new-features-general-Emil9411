using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SolarWatch.ErrorHandling;

public class JsonErrorHandling : IJsonErrorHandling
{
    public ActionResult SolarJsonError(string solarJson)
    {
        if (string.IsNullOrWhiteSpace(solarJson))
        {
            return new NotFoundObjectResult("Data not found");
        }
        
        if (solarJson.Contains("INVALID_DATE"))
        {
            return new BadRequestObjectResult("Invalid date");
        }
        
        if (solarJson.Contains("INVALID_REQUEST"))
        {
            return new BadRequestObjectResult("Invalid longitude/latitude value/s");
        }
        
        if (solarJson.Contains("UNKNOWN_ERROR"))
        {
            return new BadRequestObjectResult("Server busy, please try again later");
        }

        return new OkResult();
    }
    
    public ActionResult GeoJsonError(string geoJson)
    {
        if (string.IsNullOrWhiteSpace(geoJson) || geoJson == "[]")
        {
            return new NotFoundObjectResult("Data not found");
        }

        return new OkResult();
    }
}