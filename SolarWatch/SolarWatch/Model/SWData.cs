namespace SolarWatch.Model;

public class SWData
{
    public int Id { get; set; }
    public string? City { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Sunrise { get; set; }
    public TimeOnly Sunset { get; set; }
}