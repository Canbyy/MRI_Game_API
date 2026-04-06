namespace MriGame.Api.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Email { get; set; } = "";
}