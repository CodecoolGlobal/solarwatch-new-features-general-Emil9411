namespace SolarWatch.Services.SWServices;

public interface IJsonProcessorSw
{
    TimeOnly[] SolarJsonProcessor(string data);
}