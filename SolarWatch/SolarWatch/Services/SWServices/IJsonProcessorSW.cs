namespace SolarWatch.Services.SWServices;

public interface IJsonProcessorSW
{
    TimeOnly[] SolarJsonProcessor(string data);
}