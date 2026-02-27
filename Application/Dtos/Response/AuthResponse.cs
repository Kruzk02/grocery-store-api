namespace Application.Dtos.Response;

public record AuthResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiry);
