using System.Text.RegularExpressions;

namespace SolarWatch.Utilities;

public class NormalizeCityName : INormalizeCityName
{
    private readonly SpecialCharReplacements _specialCharReplacements;

    public NormalizeCityName(SpecialCharReplacements specialCharReplacements)
    {
        _specialCharReplacements = specialCharReplacements;
    }

    public string Normalize(string cityName)
    {
        return _specialCharReplacements.Replacements.Aggregate(cityName,
            (current, replacement) => Regex.Replace(current, replacement.Key, replacement.Value));
    }
}