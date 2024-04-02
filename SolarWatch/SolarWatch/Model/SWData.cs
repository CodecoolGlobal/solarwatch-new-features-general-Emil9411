namespace SolarWatch.Model;

public class SwData
{
    public int Id { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Sunrise { get; set; }
    public TimeOnly Sunset { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var swData = (SwData)obj;
        return City == swData.City && Date == swData.Date && Sunrise == swData.Sunrise && Sunset == swData.Sunset && Country == swData.Country && TimeZone == swData.TimeZone;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(City, Date, Sunrise, Sunset, Country, TimeZone);
    }
}