namespace SolarWatch.Model;

public class CityData
{
    public int Id { get; set; }
    public string? City { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? TimeZone { get; set; }
    public string? Country { get; set; }
}