namespace SolarWatch.Services.AuthServices;

public record AuthRequest(string EmailOrUserName, string Password);