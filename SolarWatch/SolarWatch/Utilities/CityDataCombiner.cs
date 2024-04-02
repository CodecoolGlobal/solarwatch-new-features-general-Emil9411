using SolarWatch.Model;

namespace SolarWatch.Utilities;

public class CityDataCombiner : ICityDataCombiner
{
    public CityData CombineCityData(CityData cityData, CityData timeZoneData)
    {
        return new CityData
        {
            City = cityData.City,
            Latitude = cityData.Latitude,
            Longitude = cityData.Longitude,
            TimeZone = timeZoneData.TimeZone,
            Country = timeZoneData.Country
        };
    }
}