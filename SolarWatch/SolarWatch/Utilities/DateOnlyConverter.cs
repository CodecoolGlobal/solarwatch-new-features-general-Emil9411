using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SolarWatch.Utilities;

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter() : base(
        dateOnly => dateOnly.ToDateTime(new TimeOnly()),
        dateTime => DateOnly.FromDateTime(dateTime))
    { }
}