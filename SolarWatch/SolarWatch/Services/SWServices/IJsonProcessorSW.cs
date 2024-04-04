namespace SolarWatch.Services.SwServices;

public interface IJsonProcessorSw
{
    TimeOnly[] SolarJsonProcessor(string data);
}