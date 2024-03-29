namespace SolarWatch.Model;

public class SWData
{
    public int Id { get; set; }
    public string? City { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Sunrise { get; set; }
    public TimeOnly Sunset { get; set; }
    
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var swData = (SWData)obj;
        return City == swData.City && Date == swData.Date && Sunrise == swData.Sunrise && Sunset == swData.Sunset;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(City, Date, Sunrise, Sunset);
    }
}