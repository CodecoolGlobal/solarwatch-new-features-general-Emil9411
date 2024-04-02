using SolarWatch.Model;

namespace SolarWatch.Utilities;

public interface ICityDataCombiner
{
    CityData CombineCityData(CityData cityData, CityData timeZoneData);
}