namespace SolarWatch.Services.Auth;

public record AuthRequest(string EmailOrUserName, string Password);